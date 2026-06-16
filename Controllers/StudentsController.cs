using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/students")]
public class StudentsController(IStudentService studentService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await studentService.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id) =>
        (await studentService.GetByIdAsync(id)) is StudentRecord r ? Ok(r) : NotFound();

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStudentRequest req)
    {
        var s = await studentService.CreateAsync(req.FirstName, req.LastName, req.Email);
        return CreatedAtAction(nameof(GetById), new { id = s.Id }, s);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id) =>
        await studentService.DeleteAsync(id) ? NoContent() : NotFound();
}
