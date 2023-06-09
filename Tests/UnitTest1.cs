using Microsoft.VisualStudio.TestPlatform.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void AddTask_ValidInput_ShouldAddTask()
        {
            Program.tasks.Clear();
            Program.AddTask("Task1", 10, new string[] { });
            Assert.IsTrue(Program.tasks.ContainsKey("Task1"));
        }

        [TestMethod]
        public void AddTask_InvalidInput_ShouldThrowException()
        {
            Program.tasks.Clear();
            Assert.ThrowsException<ArgumentException>(() => Program.AddTask("", 10, new string[] { }));
        }

        [TestMethod]
        public void AddTask_NullInput_ShouldThrowException()
        {
            Program.tasks.Clear();
            Assert.ThrowsException<ArgumentNullException>(() => Program.AddTask(null, 10, new string[] { }));
        }

        [TestMethod]
        public void RemoveTask_TaskExists_ShouldRemoveTask()
        {
            Program.tasks.Clear();
            Program.AddTask("Task1", 10, new string[] { });
            Program.RemoveTask("Task1");
            Assert.IsFalse(Program.tasks.ContainsKey("Task1"));
        }

        [TestMethod]
        public void RemoveTask_TaskDoesNotExist_ShouldThrowException()
        {
            Program.tasks.Clear();
            Assert.ThrowsException<ArgumentException>(() => Program.RemoveTask("Task1"));
        }

        [TestMethod]
        public void FindTaskSequence_MultipleTasks_ShouldReturnCorrectSequence()
        {
            Program.tasks.Clear();
            Program.AddTask("Task1", 10, new string[] { });
            Program.AddTask("Task2", 20, new string[] { "Task1" });
            Program.AddTask("Task3", 30, new string[] { "Task2" });

            List<Task> sequence = Program.GetTaskSequence();
            Assert.IsTrue(sequence.Select(task => task.Id).SequenceEqual(new string[] { "Task1", "Task2", "Task3" }));
        }

        [TestMethod]
        public void FindTaskSequence_NoTasks_ShouldReturnEmptySequence()
        {
            Program.tasks.Clear();
            List<Task> sequence = Program.GetTaskSequence();
            Assert.AreEqual(sequence.Count, 0);
        }
    }
}
