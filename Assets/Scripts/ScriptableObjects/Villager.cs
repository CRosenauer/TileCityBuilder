public class Villager
{
	public Building Home { set; get; }
	public Building Work { set; get; }
	public Building FoodSource { set; get; }

	public Villager(Building home)
    {
		Home = home;
	}
}
