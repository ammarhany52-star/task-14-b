using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly HealthcareContext _context;

    public AppointmentsController(HealthcareContext context)
    {
        _context = context;
    }

    // GET: api/appointments
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetAll()
    {
        var appointments = await _context.Appointments
            .AsNoTracking()
            .Select(a => new AppointmentDto
            {
                PatientId = a.PatientId,
                DoctorId = a.DoctorId,
                AppointmentDate = a.AppointmentDate
            })
            .ToListAsync();

        return Ok(appointments);
    }

    // GET: api/appointments/1/2?appointmentDate=2026-11-01T10:00:00
    [HttpGet("{patientId:int}/{doctorId:int}")]
    public async Task<ActionResult<AppointmentDto>> GetById(
        int patientId,
        int doctorId,
        [FromQuery] DateTime appointmentDate)
    {
        var appointment = await _context.Appointments
            .AsNoTracking()
            .Where(a =>
                a.PatientId == patientId &&
                a.DoctorId == doctorId &&
                a.AppointmentDate == appointmentDate)
            .Select(a => new AppointmentDto
            {
                PatientId = a.PatientId,
                DoctorId = a.DoctorId,
                AppointmentDate = a.AppointmentDate
            })
            .FirstOrDefaultAsync();

        if (appointment == null)
            return NotFound();

        return Ok(appointment);
    }

    // POST: api/appointments
    [HttpPost]
    public async Task<ActionResult<AppointmentDto>> Create(
        CreateAppointmentDto dto)
    {
        if (dto.AppointmentDate <= DateTime.Now)
            return BadRequest("Appointment date must be in the future.");

        var patientExists = await _context.Patients
            .AnyAsync(p => p.Id == dto.PatientId);

        if (!patientExists)
            return BadRequest("Patient does not exist.");

        var doctorExists = await _context.Doctors
            .AnyAsync(d => d.Id == dto.DoctorId);

        if (!doctorExists)
            return BadRequest("Doctor does not exist.");

        var alreadyBooked = await _context.Appointments
            .AnyAsync(a =>
                a.DoctorId == dto.DoctorId &&
                a.AppointmentDate == dto.AppointmentDate);

        if (alreadyBooked)
            return Conflict("The doctor is already booked at this time.");

        var appointment = new Appointment
        {
            PatientId = dto.PatientId,
            DoctorId = dto.DoctorId,
            AppointmentDate = dto.AppointmentDate
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        var result = new AppointmentDto
        {
            PatientId = appointment.PatientId,
            DoctorId = appointment.DoctorId,
            AppointmentDate = appointment.AppointmentDate
        };

        return CreatedAtAction(
            nameof(GetById),
            new
            {
                patientId = appointment.PatientId,
                doctorId = appointment.DoctorId,
                appointmentDate = appointment.AppointmentDate
            },
            result);
    }

    // PUT: api/appointments/1/2?appointmentDate=2026-11-01T10:00:00
    // The query parameter identifies the OLD appointment date.
    [HttpPut("{patientId:int}/{doctorId:int}")]
    public async Task<IActionResult> Update(
        int patientId,
        int doctorId,
        [FromQuery] DateTime appointmentDate,
        UpdateAppointmentDto dto)
    {
        var appointment = await _context.Appointments.FindAsync(
            new object[] { patientId, doctorId, appointmentDate });

        if (appointment == null)
            return NotFound();

        if (dto.AppointmentDate <= DateTime.Now)
            return BadRequest("New appointment date must be in the future.");

        var alreadyBooked = await _context.Appointments
            .AnyAsync(a =>
                a.DoctorId == doctorId &&
                a.AppointmentDate == dto.AppointmentDate &&
                (a.PatientId != patientId ||
                 a.DoctorId != doctorId ||
                 a.AppointmentDate != appointmentDate));

        if (alreadyBooked)
            return Conflict("The doctor is already booked at this time.");

        // AppointmentDate is part of the primary key.
        // EF Core does not allow changing a tracked primary key,
        // so remove the old record and insert the rescheduled record.
        var updatedAppointment = new Appointment
        {
            PatientId = appointment.PatientId,
            DoctorId = appointment.DoctorId,
            AppointmentDate = dto.AppointmentDate
        };

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        _context.Appointments.Remove(appointment);
        await _context.SaveChangesAsync();

        _context.Appointments.Add(updatedAppointment);
        await _context.SaveChangesAsync();

        await transaction.CommitAsync();

        return NoContent();
    }

    // DELETE: api/appointments/1/2?appointmentDate=2026-11-01T10:00:00
    [HttpDelete("{patientId:int}/{doctorId:int}")]
    public async Task<IActionResult> Delete(
        int patientId,
        int doctorId,
        [FromQuery] DateTime appointmentDate)
    {
        var appointment = await _context.Appointments.FindAsync(
            new object[] { patientId, doctorId, appointmentDate });

        if (appointment == null)
            return NotFound();

        _context.Appointments.Remove(appointment);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}