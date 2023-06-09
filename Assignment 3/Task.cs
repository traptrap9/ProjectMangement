class Task
{
    public string Id { get; set; }
    public int Time { get; set; }
    public List<string> Dependencies { get; set; }

    public Task(string id, int time, List<string> dependencies)
    {
        Id = id;
        Time = time;
        Dependencies = dependencies;
    }
}