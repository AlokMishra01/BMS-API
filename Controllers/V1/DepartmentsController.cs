using Asp.Versioning;
using IdentityManager.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BMS_API.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/{id:guid}/departments")]
    [ApiController]
    [Authorize]
    public class DepartmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DepartmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // // GET: api/v1/departments
        // [HttpGet]
        // public async Task<ActionResult<ApiResponse<IEnumerable<DepartmentResponseDTO>>>> GetDepartments()
        // {
        //     var departments = await _context.Departments.ToListAsync();
        // 
        //     var response = departments.Select(d => new DepartmentResponseDTO
        //     {
        //         Id = d.Id,
        //         DepartmentName = d.DepartmentName,
        //         Description = d.Description,
        //         CreatedAt = d.CreatedAt,
        //         UpdatedAt = d.UpdatedAt
        //     });
        // 
        //     return Ok(new ApiResponse<IEnumerable<DepartmentResponseDTO>>
        //     {
        //         Success = true,
        //         Message = "Departments retrieved successfully",
        //         Data = response.ToList()
        //     });
        // }
        // 
        // // GET: api/v1/departments/{id}
        // [HttpGet("{id:guid}")]
        // public async Task<ActionResult<ApiResponse<DepartmentResponseDTO>>> GetDepartment(Guid id)
        // {
        //     var department = await _context.Departments.FindAsync(id);
        // 
        //     if (department == null)
        //     {
        //         return NotFound(new ApiResponse<DepartmentResponseDTO>
        //         {
        //             Success = false,
        //             Message = "Department not found"
        //         });
        //     }
        // 
        //     var response = new DepartmentResponseDTO
        //     {
        //         Id = department.Id,
        //         DepartmentName = department.DepartmentName,
        //         Description = department.Description,
        //         CreatedAt = department.CreatedAt,
        //         UpdatedAt = department.UpdatedAt
        //     };
        // 
        //     return Ok(new ApiResponse<DepartmentResponseDTO>
        //     {
        //         Success = true,
        //         Message = "Department retrieved successfully",
        //         Data = response
        //     });
        // }
        // 
        // // POST: api/v1/departments
        // [HttpPost]
        // public async Task<ActionResult<ApiResponse<DepartmentResponseDTO>>> CreateDepartment([FromBody] DepartmentCreateDTO model)
        // {
        //     if (model == null || string.IsNullOrWhiteSpace(model.DepartmentName) || string.IsNullOrWhiteSpace(model.Description))
        //     {
        //         return BadRequest(new ApiResponse<DepartmentResponseDTO>
        //         {
        //             Success = false,
        //             Message = "Department name and description are required."
        //         });
        //     }
        // 
        //     var newDepartment = new Department
        //     {
        //         DepartmentName = model.DepartmentName,
        //         Description = model.Description,
        //         CreatedAt = DateTime.UtcNow,
        //         UpdatedAt = DateTime.UtcNow
        //     };
        // 
        //     _context.Departments.Add(newDepartment);
        //     await _context.SaveChangesAsync();
        // 
        //     var response = new DepartmentResponseDTO
        //     {
        //         Id = newDepartment.Id,
        //         DepartmentName = newDepartment.DepartmentName,
        //         Description = newDepartment.Description,
        //         CreatedAt = newDepartment.CreatedAt,
        //         UpdatedAt = newDepartment.UpdatedAt
        //     };
        // 
        //     return CreatedAtAction(nameof(GetDepartment), new { id = newDepartment.Id }, new ApiResponse<DepartmentResponseDTO>
        //     {
        //         Success = true,
        //         Message = "Department created successfully",
        //         Data = response
        //     });
        // }
        // 
        // // PUT: api/v1/departments/{id}
        // [HttpPut("{id:guid}")]
        // public async Task<ActionResult<ApiResponse<DepartmentResponseDTO>>> UpdateDepartment(Guid id, [FromBody] DepartmentUpdateDTO model)
        // {
        //     if (model == null || string.IsNullOrWhiteSpace(model.DepartmentName) || string.IsNullOrWhiteSpace(model.Description))
        //     {
        //         return BadRequest(new ApiResponse<DepartmentResponseDTO>
        //         {
        //             Success = false,
        //             Message = "Department name and description are required."
        //         });
        //     }
        // 
        //     var department = await _context.Departments.FindAsync(id);
        //     if (department == null)
        //     {
        //         return NotFound(new ApiResponse<DepartmentResponseDTO>
        //         {
        //             Success = false,
        //             Message = "Department not found."
        //         });
        //     }
        // 
        //     department.DepartmentName = model.DepartmentName;
        //     department.Description = model.Description;
        //     department.UpdatedAt = DateTime.UtcNow;
        // 
        //     await _context.SaveChangesAsync();
        // 
        //     var response = new DepartmentResponseDTO
        //     {
        //         Id = department.Id,
        //         DepartmentName = department.DepartmentName,
        //         Description = department.Description,
        //         CreatedAt = department.CreatedAt,
        //         UpdatedAt = department.UpdatedAt
        //     };
        // 
        //     return Ok(new ApiResponse<DepartmentResponseDTO>
        //     {
        //         Success = true,
        //         Message = "Department updated successfully",
        //         Data = response
        //     });
        // }
        // 
        // // DELETE: api/v1/departments/{id}
        // [HttpDelete("{id:guid}")]
        // public async Task<ActionResult<ApiResponse<DepartmentResponseDTO>>> DeleteDepartment(Guid id)
        // {
        //     var department = await _context.Departments.FindAsync(id);
        //     if (department == null)
        //     {
        //         return NotFound(new ApiResponse<DepartmentResponseDTO>
        //         {
        //             Success = false,
        //             Message = "Department not found."
        //         });
        //     }
        // 
        //     _context.Departments.Remove(department);
        //     await _context.SaveChangesAsync();
        // 
        //     return Ok(new ApiResponse<DepartmentResponseDTO>
        //     {
        //         Success = true,
        //         Message = "Department deleted successfully"
        //     });
        // }
    }
}
