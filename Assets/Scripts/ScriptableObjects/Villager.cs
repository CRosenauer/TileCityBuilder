public class Villager
{
	public Building Home { set; get; }
	public Building Work { set; get; }
	public Building FoodSource { set; get; }

	public bool HasJob { get { return Work != null; }  }
	public bool HasFood { get { return FoodSource != null; }  }

	public Villager(Building home)
    {
		Home = home;
	}
}
