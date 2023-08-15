using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

class Elevator
{
    private int currentFloor;
    private Direction direction;
    private bool isMoving;
    private bool maxWeightReached;
    private List<int> requestedFloors;

    public event EventHandler<int> FloorPassed;
    public event EventHandler<int> FloorStopped;

    public Elevator()
    {
        currentFloor = 1;
        direction = Direction.None;
        isMoving = false;
        maxWeightReached = false;
        requestedFloors = new List<int>();
    }

    public void RequestFloor(int floor)
    {
        lock (requestedFloors)
        {
            if (!requestedFloors.Contains(floor) && currentFloor != floor)
            {
                requestedFloors.Add(floor);
                LogAsync($"Requested floor: {floor}");
                if (!isMoving)
                {
                    StartMoving();
                }
            }
        }
    }

    private void StartMoving()
    {
        isMoving = true;
        direction = currentFloor < requestedFloors[0] ? Direction.Up : Direction.Down;
        LogAsync($"Elevator started moving {direction}");
        MoveToNextFloor();
    }

    private async void MoveToNextFloor()
    {
        while (requestedFloors.Count > 0)
        {
            if (maxWeightReached)
            {
                LogAsync("Max weight reached. Elevator stopped.");
                isMoving = false;
                direction = Direction.None;
                await Task.Delay(1000);
                maxWeightReached = false;
            }

            if (direction == Direction.Up)
            {
                currentFloor++;
            }
            else if (direction == Direction.Down)
            {
                currentFloor--;
            }

            FloorPassed?.Invoke(this, currentFloor);
            LogAsync($"Passed floor: {currentFloor}");

            if (requestedFloors.Contains(currentFloor))
            {
                requestedFloors.Remove(currentFloor);
                FloorStopped?.Invoke(this, currentFloor);
                LogAsync($"Stopped at floor: {currentFloor}");
                await Task.Delay(1000);
            }

            await Task.Delay(3000);
        }

        isMoving = false;
        direction = Direction.None;
        LogAsync("Elevator stopped moving");
    }

    public void SensorData(bool isMoving, int currentFloor, Direction direction, bool maxWeightReached)
    {
        this.isMoving = isMoving;
        this.currentFloor = currentFloor;
        this.direction = direction;
        this.maxWeightReached = maxWeightReached;
    }

    private async void LogAsync(string message)
    {
        string log = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
        using (StreamWriter writer = File.AppendText("elevator_log.txt"))
        {
            await writer.WriteLineAsync(log);
        }
    }
}

enum Direction
{
    None,
    Up,
    Down
}

class Program
{
    static void Main(string[] args)
    {
        Elevator elevator = new Elevator();
        elevator.FloorPassed += (sender, floor) =>
        {
            Console.WriteLine($"Passed floor: {floor}");
        };
        elevator.FloorStopped += (sender, floor) =>
        {
            Console.WriteLine($"Stopped at floor: {floor}");
        };

        elevator.RequestFloor(5);
        elevator.RequestFloor(2);
        elevator.RequestFloor(7);

        Thread.Sleep(20000); // Simulate some time passing

        // Simulate sensor data
        elevator.SensorData(false, 3, Direction.None, false);

        elevator.RequestFloor(1);

        Thread.Sleep(10000); // Simulate some time passing

        // Simulate sensor data
        elevator.SensorData(true, 1, Direction.Up, false);

        Thread.Sleep(15000); // Simulate some time passing

        // Simulate sensor data
        elevator.SensorData(false, 6, Direction.None, false);

        elevator.RequestFloor(3);

        Thread.Sleep(5000); // Simulate some time passing

        // Simulate sensor data
        elevator.SensorData(true, 6, Direction.Down, false);

        Thread.Sleep(10000); // Simulate some time passing
    }
}

