using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;

[ApiController]
[Route("api/[controller]")]
public class DoctorsController : ControllerBase
{
    private readonly HealthcareContext _context;

    public DoctorsController(HealthcareContext context)
    {
        _context = context;
    }

    // GET: api/doctors
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DoctorDto>>> GetAll()
    {
        var doctors = await _context.Doctors
            .AsNoTracking()
            .Select(d => new DoctorDto
            {
                Id = d.Id,
                Name = d.Name,
                Specialization = d.Specialization
            })
            .ToListAsync();

        return Ok(doctors);
    }

    // GET: api/doctors/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<DoctorDto>> GetById(int id)
    {
        var doctor = await _context.Doctors
            .AsNoTracking()
            .Where(d => d.Id == id)
            .Select(d => new DoctorDto
            {
                Id = d.Id,
                Name = d.Name,
                Specialization = d.Specialization
            })
            .FirstOrDefaultAsync();

        if (doctor == null)
            return NotFound();

        return Ok(doctor);
    }

    // POST: api/doctors
    [HttpPost]
    public async Task<ActionResult<DoctorDto>> Create(
        CreateDoctorDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) ||
            string.IsNullOrWhiteSpace(dto.Specialization))
        {
            return BadRequest("Name and Specialization are required.");
        }

        var doctor = new Doctor
        {
            Name = dto.Name,
            Specialization = dto.Specialization
        };

        _context.Doctors.Add(doctor);
        await _context.SaveChangesAsync();

        var result = new DoctorDto
        {
            Id = doctor.Id,
            Name = doctor.Name,
            Specialization = doctor.Specialization
        };

        return CreatedAtAction(
            nameof(GetById),
            new { id = doctor.Id },
            result);
    }

    // PUT: api/doctors/1
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id, UpdateDoctorDto dto)
    {
        var doctor = await _context.Doctors.FindAsync(id);

        if (doctor == null)
            return NotFound();

        if (string.IsNullOrWhiteSpace(dto.Name) ||
            string.IsNullOrWhiteSpace(dto.Specialization))
        {
            return BadRequest("Name and Specialization are required.");
        }

        doctor.Name = dto.Name;
        doctor.Specialization = dto.Specialization;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/doctors/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var doctor = await _context.Doctors.FindAsync(id);

        if (doctor == null)
            return NotFound();

        if (await _context.Appointments.AnyAsync(
            a => a.DoctorId == id))
        {
            return Conflict(
                "Cannot delete a doctor who has appointments.");
        }

        _context.Doctors.Remove(doctor);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}