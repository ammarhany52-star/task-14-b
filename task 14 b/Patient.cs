
public class Patient
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public DateTime DateOfBirth { get; set; }

    public ICollection<Appointment> Appointments
    { get; set; } = new List<Appointment>();
}
