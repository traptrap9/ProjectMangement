using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;
using static System.Net.WebRequestMethods;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using System.Data;

class Program
{
    public static Dictionary<string, Task> tasks = new Dictionary<string, Task>();

    static void Main(string[] args)
    {
        int command;
        do
        {
            Console.WriteLine("------------------------------------------");
            Console.WriteLine("Please enter a command number:");
            Console.WriteLine("1. Load tasks from a file");
            Console.WriteLine("2. Add a new task");
            Console.WriteLine("3. Remove a task");
            Console.WriteLine("4. Change the time needed for a task");
            Console.WriteLine("5. Save tasks to the file");
            Console.WriteLine("6. Find a sequence of tasks");
            Console.WriteLine("7. Find earliest commencement times for tasks");
            Console.WriteLine("8. Exit the program");
            Console.WriteLine("9. Print all tasks");

            while (!int.TryParse(Console.ReadLine(), out command) || command < 1 || command > 9)
            {
                Console.WriteLine("Invalid input. Please enter a number between 1 and 9.");
            }

            switch (command)
            {
                case 1:
                    Console.Write("Enter txt filename: ");
                    string filename = "../../../" + Console.ReadLine();
                    // Make sure the file has a .txt extension
                    if (!filename.EndsWith(".txt"))
                    {
                        filename += ".txt";
                    }
                    LoadTasksFromFile(filename);
                    break;
                case 2:
                    // Collect data for new task
                    Console.Write("Enter task ID: ");
                    string id = Console.ReadLine().ToUpper();
                    Console.Write("Enter task time: ");
                    int time;
                    while (!int.TryParse(Console.ReadLine(), out time))
                    {
                        Console.Write("Invalid input. Please enter a number for the task time: ");
                    }
                    Console.Write("Enter (any or none) task dependencies (comma separated): ");
                    string[] dependencies = Console.ReadLine().ToUpper().Split(',');
                    AddTask(id, time, dependencies);
                    break;
                case 3:
                    Console.Write("Enter task ID to remove: ");
                    string removeId = Console.ReadLine().ToUpper();
                    RemoveTask(removeId);
                    break;
                case 4:
                    Console.Write("Enter task ID to change: ");
                    string changeId = Console.ReadLine().ToUpper();
                    Console.Write("Enter new time for task: ");
                    int newTime;
                    while (!int.TryParse(Console.ReadLine(), out newTime) || newTime <= 0)
                    {
                        Console.Write("Invalid input. Please enter a positive number for the task time: ");
                    }
                    ChangeTaskTime(changeId, newTime);
                    break;
                case 5:
                    Console.Write("Enter txt filename to save tasks: ");
                    string saveFilename = Console.ReadLine();

                    // Make sure the file has a .txt extension
                    if (!saveFilename.EndsWith(".txt"))
                    {
                        saveFilename += ".txt";
                    }

                    saveFilename = "../../../" + saveFilename;

                    SaveTasksToFile(saveFilename);
                    break;
                case 6:
                    FindTaskSequence();
                    break;
                case 7:
                    FindEarliestTimes();
                    break;
                case 8:
                    Console.WriteLine("Exiting the program.");
                    break;
                case 9:
                    PrintAllTasks();
                    break;
                default:
                    Console.WriteLine("Invalid command. Please try again.");
                    break;
            }
        } while (command != 8);
    }

    static void PrintAllTasks()
    {
        Console.WriteLine("");
        if (tasks.Count == 0)
        {
            Console.WriteLine("No tasks loaded.");
            return;
        }

        Console.WriteLine("Loaded tasks:");
        foreach (KeyValuePair<string, Task> task in tasks)
        {
            Console.Write($"Task ID: {task.Value.Id}, Time: {task.Value.Time}, Dependencies: ");
            foreach (string dependency in task.Value.Dependencies)
            {
                Console.Write($"{dependency} ");
            }
            Console.WriteLine();
        }
    }



    static void LoadTasksFromFile(string fileName)
    {
        tasks.Clear();
        if (!System.IO.File.Exists(fileName))
        {
            Console.WriteLine($"File {fileName} does not exist.");
            return;
        }

        try
        {
            string[] lines = System.IO.File.ReadAllLines(fileName);

            foreach (string line in lines)
            {
                string[] parts = line.Split(',');
                if (parts.Length < 2)
                {
                    Console.WriteLine($"Invalid line in file: {line}\n");
                    continue;
                }

                string id = parts[0].Trim();
                if (!int.TryParse(parts[1].Trim(), out int time))
                {
                    Console.WriteLine($"Invalid time in line: {line}\n");
                    continue;
                }

                List<string> dependencies = new List<string>();
                for (int i = 2; i < parts.Length; i++)
                {
                    dependencies.Add(parts[i].Trim());
                }

                Task task = new Task(id, time, dependencies);
                tasks[id] = task;
            }

            PrintAllTasks();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while reading the file: {ex.Message}\n");
        }
    }


    static void AddTask(string id, int time, string[] dependencyIds)
    {
        // Check if the task ID already exists
        if (tasks.ContainsKey(id))
        {
            Console.WriteLine($"Task ID {id} already exists. Please choose a different ID.");
            return;
        }

        // Check if the task ID is empty or white space
        if (string.IsNullOrWhiteSpace(id))
        {
            Console.WriteLine("Task ID must not be empty. Please enter a valid ID.");
            return;
        }

            // Check if the time is a positive number
            if (time < 0)
        {
            Console.WriteLine($"Task time must be a positive number. You entered: {time}");
            return;
        }

        // Convert the dependency IDs array to a List of strings, and trim any white space
        List<string> dependencies = new List<string>();

        // Only add dependencies if they are not empty strings
        if (dependencyIds.Length != 1 || !string.IsNullOrEmpty(dependencyIds[0]))
        {
            dependencies = dependencyIds.Select(d => d.Trim()).ToList();

            // Check if all dependencies exist
            foreach (string dependencyId in dependencies)
            {
                if (!tasks.ContainsKey(dependencyId))
                {
                    Console.WriteLine($"Dependency task ID {dependencyId} does not exist. Task was not added.");
                    return;
                }
            }
        }

        // If all checks have passed, add the new task
        Task newTask = new Task(id, time, dependencies);
        tasks[id] = newTask;

        Console.WriteLine($"Task {id} has been successfully added.");
    }


    public static void RemoveTask(string taskId)
    {
        // Check if the task exists
        if (!tasks.ContainsKey(taskId))
        {
            Console.WriteLine($"Task with ID {taskId} does not exist.");
            return;
        }

        // Remove the task
        tasks.Remove(taskId);

        // Remove the task from the dependency list of all remaining tasks
        foreach (var task in tasks.Values)
        {
            task.Dependencies.Remove(taskId);
        }

        Console.WriteLine($"Task with ID {taskId} has been removed successfully.");
    }


    static void ChangeTaskTime(string id, int newTime)
    {
        try
        {
            if (!tasks.ContainsKey(id))
            {
                Console.WriteLine($"Task with ID {id} does not exist.");
                return;
            }

            if (newTime <= 0)
            {
                throw new ArgumentException("Task time must be greater than zero.");
            }

            Task task = tasks[id];
            task.Time = newTime;
            Console.WriteLine($"The time for task {id} has been successfully changed to {newTime}.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred during ChangeTaskTime: {ex.Message}");
        }
    }


    static void SaveTasksToFile(string filename, List<Task> sequence = null, SortedDictionary<string, int> earliestTimes = null)
    {
        string execDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string filePath = Path.Combine(execDirectory, filename);

        try
        {
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                if (sequence != null)
                {
                    sw.WriteLine(string.Join(", ", sequence.Select(task => task.Id)));
                }
                else if (earliestTimes != null)
                {
                    foreach (var kvp in earliestTimes)
                    {
                        sw.WriteLine($"{kvp.Key}, {kvp.Value}");
                    }
                }
                else
                {
                    foreach (KeyValuePair<string, Task> task in tasks)
                    {
                        sw.Write($"{task.Value.Id}, {task.Value.Time}");
                        foreach (string dependency in task.Value.Dependencies)
                        {
                            sw.Write($", {dependency}");
                        }
                        sw.WriteLine();
                    }
                }
            }
            Console.WriteLine($"Tasks have been saved to {Path.GetFullPath(filePath)}.");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while saving tasks to the file: {ex.Message}");
        }
    }

    public static List<Task> GetTaskSequence()
    {
        // Create a copy of the tasks dictionary to avoid modifying the original
        Dictionary<string, Task> tasksCopy = new Dictionary<string, Task>(tasks);
        // Prepare the result list
        List<Task> taskSequence = new List<Task>();

        // While there are tasks left to schedule
        while (tasksCopy.Count > 0)
        {
            // Find a task with no unscheduled dependencies
            Task nextTask = tasksCopy.Values.FirstOrDefault(task => task.Dependencies.All(dependency => !tasksCopy.ContainsKey(dependency)));
            // If no such task can be found, then there's a circular dependency
            if (nextTask == null)
            {
                return null;  // Indicates a circular dependency
            }
            // Add the found task to the sequence and remove it from the copy
            taskSequence.Add(nextTask);
            tasksCopy.Remove(nextTask.Id);
        }
        return taskSequence;
    }

    public static void FindTaskSequence()
    {
        try
        {
            // Get the sequence of tasks
            List<Task> taskSequence = GetTaskSequence();
            if (taskSequence == null)
            {
                Console.WriteLine("Circular dependency detected. No valid task sequence can be found.");
                return;
            }

            // Print the task sequence
            Console.WriteLine("The found task sequence is:");
            foreach (Task task in taskSequence)
            {
                Console.WriteLine($"Task ID: {task.Id}, Time: {task.Time}, Dependencies: {string.Join(" ", task.Dependencies)}");
            }

            // Save the sequence to a file
            SaveTasksToFile("../../../Sequence.txt", taskSequence);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred during FindTaskSequence: {ex.Message}");
        }

    }

    public static void FindEarliestTimes()
    {
        try
        {
            // Get the sequence of tasks
            List<Task> taskSequence = GetTaskSequence();
            if (taskSequence == null)
            {
                Console.WriteLine("Circular dependency detected. No valid task sequence can be found.");
                return;
            }

            SortedDictionary<string, int> earliestTimes = new SortedDictionary<string, int>();

            foreach (var task in taskSequence)
            {
                // Initialise the earliest time for the current task as 0
                int earliestTimeForThisTask = 0;
                foreach (var dependencyId in task.Dependencies)
                {
                    int dependencyEndTime = earliestTimes[dependencyId] + tasks[dependencyId].Time;
                    if (dependencyEndTime > earliestTimeForThisTask)
                    {
                        earliestTimeForThisTask = dependencyEndTime;
                    }
                }
                earliestTimes[task.Id] = earliestTimeForThisTask;
            }

            // Convert the dictionary to a list and sort it by the numeric value of the task ID
            var sortedList = earliestTimes.ToList();
            sortedList.Sort((pair1, pair2) => int.Parse(pair1.Key.Substring(1)).CompareTo(int.Parse(pair2.Key.Substring(1))));

            // Print the earliest commencement times in sorted order
            foreach (var kvp in sortedList)
            {
                Console.WriteLine($"{kvp.Key}, {kvp.Value}");
            }

            // Save the earliest commencement times to a file
            SaveEarliestTimesToFile("../../../EarliestTimes.txt", sortedList);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred during FindEarliestTimes: {ex.Message}");
        }
    }

    public static void SaveEarliestTimesToFile(string path, List<KeyValuePair<string, int>> taskDict)
    {
        try
        {
            using (StreamWriter sw = new StreamWriter(path))
            {
                if (taskDict != null)
                {
                    foreach (var kvp in taskDict)
                    {
                        sw.WriteLine($"{kvp.Key}, {kvp.Value}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred during SaveEarliestTimesToFile: {ex.Message}");
        }
    }



}


