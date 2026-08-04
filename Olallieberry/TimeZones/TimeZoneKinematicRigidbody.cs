namespace Olallieberry.TimeZones;

public class TimeZoneKinematicRigidbody : TimeZoneRigidbody
{
	protected override void ConfigureRigidbody()
	{
		_rigidbody.MakeKinematic();
		_rigidbody.EnableKinematicSimulation();
	}
}
