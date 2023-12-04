class CustomerService
{
	internal class Customer
	{
		public int startTime, creationTime;
		private string name;
		private int _completionTime;
		private bool _premium;
		public int completionTime
		{
			get { return _completionTime; }
		}

		public bool premium
		{
			get { return _premium; }
		}

		public Customer()
		{
			name = new Random().Next(0, 100).ToString();
			_completionTime = new Random().Next(10, 20);
			_premium = new Random().NextDouble() >= 0.5d;
		}

		public override string ToString()
		{
			return string.Format("Customer ({0})", name);
		}
	}

	internal class Representative
	{
		public Customer? assignment = null;
		private string name;

		public Representative()
		{
			name = new Random().Next(0, 100).ToString();
		}

		public override string ToString()
		{
			return string.Format("Representative ({0})", name);
		}
	}

	private List<Representative> representatives;
	private List<Customer> queue = new List<Customer>(), priorityQueue = new List<Customer>();
	private int spawnTime, trialCount, stepTime;
	private int timer = 0;
	private List<int> waitTimes = new List<int>(), priorityWaitTimes = new List<int>();

	public CustomerService(int representativeCount)
	{
		representatives = new List<Representative>();

		for (int i = 0; i < representativeCount; i++)
		{
			representatives.Add(new());
		}
	}

	public static void Main(string[] args)
	{
		CustomerService customerService = new(4) { trialCount = 10000, stepTime = 1, spawnTime = 3 };

		customerService.PrintRepresentatives();
		customerService.StartService();
	}

	public void StartService()
	{
		bool finished = false;

		while (timer < trialCount || queue.Count > 0 || priorityQueue.Count > 0)
		{
			Thread.Sleep(stepTime);

			timer++;

			{
				Representative? representative = FindOpenRepresentative();

				if ((priorityQueue.Count > 0 || queue.Count > 0) && representative != null)
				{
					List<Customer> targetQueue = priorityQueue.Count > 0 ? priorityQueue : queue;
					Customer customer = targetQueue[targetQueue.Count - 1];

					AssignTo(representative, customer);

					Console.WriteLine(string.Format("Found an open representative for {0} ({1})! They are now being removed from the {2}queue.", customer, representative, customer.premium ? "premium " : ""));
					
					targetQueue.Remove(customer);
				}
			}

			foreach (Representative representative in representatives)
			{
				if (representative.assignment != null && timer - representative.assignment.completionTime >= representative.assignment.startTime)
				{
					Console.WriteLine(string.Format("\t{0} finished with {1} and is now open!", representative, representative.assignment));

					representative.assignment = null;
				}
			}

			if (!finished && timer % spawnTime == 0)
			{
				Customer customer = new Customer();

				customer.creationTime = timer;

				Console.WriteLine(string.Format("New {0}", customer));

				AddCustomer(customer);
			}

			if (timer >= trialCount && !finished)
			{
				Console.WriteLine("\t>> TRIAL OVER! FINISHING REMAINING CUSTOMERS <<");
				finished = true;
			}
        }

		PrintStatistics();
	}

	private Representative? FindOpenRepresentative()
	{
		return representatives.Find((Representative searchRepresentative) => searchRepresentative.assignment == null);
	}

	private void AssignTo(Representative representative, Customer customer)
	{
		representative.assignment = customer;
		customer.startTime = timer;

		if (customer.premium)
		{
			priorityWaitTimes.Add(timer - customer.creationTime);
		} else
		{
			waitTimes.Add(timer - customer.creationTime);
		}
	}

	public void AddCustomer(Customer customer)
	{
		Representative? representative = FindOpenRepresentative();

		if (representative != null)
		{
			AssignTo(representative, customer);

			Console.WriteLine(string.Format("{0} was assigned to {1}!", customer, representative));
		} else
		{
			List<Customer> targetQueue = customer.premium ? priorityQueue : queue;

			targetQueue.Add(customer);

			Console.WriteLine(string.Format("No representatives available for {0}, they have been added to the {1}queue.", customer, customer.premium ? "premium " : ""));
		}
	}

	public void PrintRepresentatives()
	{
		foreach (Representative representative in representatives)
		{
			Console.WriteLine(representative);
		}
	}

	public void PrintStatistics()
	{
		
		Console.WriteLine(string.Format("Average non-priority wait time: {0}\nAverage priority wait time: {1}\nTotal average wait time: {2}", waitTimes.Average(), priorityWaitTimes.Average(), waitTimes.Concat(priorityWaitTimes).Average()));
	}
}