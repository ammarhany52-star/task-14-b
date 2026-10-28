using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;

[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly HealthcareContext _context;

    public PatientsController(HealthcareContext context)
    {
        _context = context;
    }

    // GET: api/patients
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PatientDto>>> GetAll()
    {
        var patients = await _context.Patients
            .AsNoTracking()
            .Select(p => new PatientDto
            {
                Id = p.Id,
                Name = p.Name,
                DateOfBirth = p.DateOfBirth
            })
            .ToListAsync();

        return Ok(patients);
    }

    // GET: api/patients/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PatientDto>> GetById(int id)
    {
        var patient = await _context.Patients
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new PatientDto
            {
                Id = p.Id,
                Name = p.Name,
                DateOfBirth = p.DateOfBirth
            })
            .FirstOrDefaultAsync();

        if (patient == null)
            return NotFound();

        return Ok(patient);
    }

    // POST: api/patients
    [HttpPost]
    public async Task<ActionResult<PatientDto>> Create(
        CreatePatientDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Patient name is required.");

        if (dto.DateOfBirth.Date > DateTime.Today)
            return BadRequest("Date of birth cannot be in the future.");

        var patient = new Patient
        {
            Name = dto.Name,
            DateOfBirth = dto.DateOfBirth
        };

        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();

        var result = new PatientDto
        {
            Id = patient.Id,
            Name = patient.Name,
            DateOfBirth = patient.DateOfBirth
        };

        return CreatedAtAction(
            nameof(GetById),
            new { id = patient.Id },
            result);
    }

    // PUT: api/patients/1
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id, UpdatePatientDto dto)
    {
        var patient = await _context.Patients.FindAsync(id);

        if (patient == null)
            return NotFound();

        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Patient name is required.");

        if (dto.DateOfBirth.Date > DateTime.Today)
            return BadRequest("Date of birth cannot be in the future.");

        patient.Name = dto.Name;
        patient.DateOfBirth = dto.DateOfBirth;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/patients/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var patient = await _context.Patients.FindAsync(id);

        if (patient == null)
            return NotFound();

        if (await _context.Appointments.AnyAsync(
            a => a.PatientId == id))
        {
            return Conflict(
                "Cannot delete a patient who has appointments.");
        }

        _context.Patients.Remove(patient);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}