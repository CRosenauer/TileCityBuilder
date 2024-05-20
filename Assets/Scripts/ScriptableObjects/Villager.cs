public class Villager
{
	public Building Home { set; get; }
	public Building Work { set; get; }
	public Building FoodSource { set; get; }
	public Building EntertainmentSource { set; get; }
	public Building ReligionSource { set; get; }

	public bool HasJob { get { return Work != null; }  }
	public bool HasFood { get { return FoodSource != null; } }
	public bool HasEntertainment { get { return EntertainmentSource != null; } }
	public bool HasChurch { get { return ReligionSource != null; } }

	public Villager(Building home)
    {
		Home = home;
	}
}
