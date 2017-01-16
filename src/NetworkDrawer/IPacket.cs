namespace NetworkDrawing
{
	public interface IPacket
	{
		ISubject From { get; }
		PacketState State { get; }
		ISubject To { get; }
	}
}
