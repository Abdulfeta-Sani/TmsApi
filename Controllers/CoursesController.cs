using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/courses")]
public class CoursesController(ICourseService courseService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await courseService.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id) =>
        (await courseService.GetByIdAsync(id)) is CourseRecord r ? Ok(r) : NotFound();

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCourseRequest req)
    {
        var c = await courseService.CreateAsync(req.Code, req.Title, req.Description);
        return CreatedAtAction(nameof(GetById), new { id = c.Id }, c);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id) =>
        await courseService.DeleteAsync(id) ? NoContent() : NotFound();
}