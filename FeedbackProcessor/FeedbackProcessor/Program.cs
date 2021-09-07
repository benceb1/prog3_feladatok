﻿using System;
using System.Collections.Generic;
using System.IO;

namespace FeedbackProcessor
{
    enum Category
    {
        Opinion,
        FeatureRequest,
        BugReport
    }

    class Feedback
    {
        public Category Category { get; set; }
        public int Priority { get; set; }
        public string Product { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return $"\t[{Category} of priority {Priority}]\n\t*** {Product} ***\n\t{Description}";
        }
        // javasolt
        public override bool Equals(object obj)
        {
            if (obj is Feedback)
            {   // gyors
                Feedback other = obj as Feedback;
                return this.Category == other.Category &&
                    this.Priority == other.Priority &&
                    this.Product == other.Product &&
                    this.Description == other.Description;
            } else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            // return 0; // force equals
            return (int)Category + Priority + Product.GetHashCode() + Description.GetHashCode();
        }
    }

    class FeedbackProcessor
    {
        const int LIMIT = 3;
        List<Feedback> feedbacks;
        Dictionary<Category, Action<Feedback>> feedbackActions;

        public FeedbackProcessor(Action<Feedback> defaultAction)
        {
            if (defaultAction == null)
            {
                throw new ArgumentNullException(nameof(defaultAction));
            }
            feedbacks = new List<Feedback>();
            feedbackActions = new Dictionary<Category, Action<Feedback>>();
            foreach (Category item in Enum.GetValues(typeof(Category)))
            {
                feedbackActions.Add(item, defaultAction);
            }
        }

        public void AddAction(Category cat, Action<Feedback> method, bool doOverwrite)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }
            if (doOverwrite)
            {
                feedbackActions[cat] = method;
            } else
            {
                feedbackActions[cat] += method;
            }
        }

        public void AddFeedback(Feedback feedback)
        {
            feedbacks.Add(feedback);
            if (feedbacks.Count == LIMIT)
            {
                foreach (Feedback item in feedbacks)
                {
                    // hibát dob, ha a hozzáadásnál az action null, szóval nem kell a '?.'
                    feedbackActions[item.Category]?.Invoke(item);
                    Console.WriteLine("!!! FEEDBACK PROCESSED !!!");
                }
                feedbacks.Clear();
            }
        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            FeedbackProcessor processor = new FeedbackProcessor((fb) =>
            {
                Console.WriteLine($"Nothing yet for the feedback of {fb.Category}");
            });

            Feedback fb1 = new Feedback()
            {
                Category = Category.BugReport,
                Priority = 5,
                Product = "Firefox",
                Description = "This s**t froze!"
            };
            Feedback fb2 = new Feedback()
            {
                Category = Category.FeatureRequest,
                Priority = 9,
                Product = "Chrome",
                Description = "Make it less bloatware!"
            };
            Feedback fb3 = new Feedback()
            {
                Category = Category.Opinion,
                Priority = 1,
                Product = "Edge",
                Description = "This must die!"
            };

            processor.AddFeedback(fb1);
            processor.AddFeedback(fb2);
            processor.AddFeedback(fb3);

            Console.ReadLine();

            // másik hozzáadása
            processor.AddAction(Category.BugReport, WriteToConsole, true);
            processor.AddAction(Category.FeatureRequest, WriteToConsole, true);
            processor.AddAction(Category.Opinion, WriteToConsole, true);

            // másik módszer
            Action<Feedback> saveIntoFile = (fb) =>
            {
                File.AppendAllText("feedbacks.log", fb.ToString().Replace("\t", "") + "\n\n");
                Console.WriteLine("*** FILE!");
            };
            processor.AddAction(Category.BugReport, saveIntoFile, false);
            processor.AddAction(Category.FeatureRequest, saveIntoFile, false);

            // bugreportot e-mailben is elküldjük
            string destination = "info@nik.uni-obuda.hu";
            processor.AddAction(Category.BugReport, (fb) => {
                // a string változót referencia szerint látja
                Console.WriteLine($"*** EMAIL to {destination} ***");
                Console.WriteLine(fb.ToString());
                Console.WriteLine("*** EMAIL ENDS ***");
            }, false);

            destination = "asd@asd.com";
            processor.AddFeedback(fb1);
            processor.AddFeedback(fb2);
            processor.AddFeedback(fb3);
            Console.ReadLine();
        }

        static void WriteToConsole(Feedback fb)
        {
            Console.WriteLine("*** CONSOLE ***");
            Console.WriteLine(fb.ToString());
            Console.WriteLine("*** CONSOLE ***");

        }
    }
}
