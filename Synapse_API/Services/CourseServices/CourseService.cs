using AutoMapper;
using Synapse_API.Models.Dto.CourseDTOs;
using Synapse_API.Models.Dto.EventDTOs;
using Synapse_API.Models.Entities;
using Synapse_API.Repositories.Course;

namespace Synapse_API.Services.CourseServices
{
    public class CourseService
    {
        private readonly CourseRepository _courseRepository;
        private readonly IMapper _mapper;
        public CourseService(CourseRepository courseRepository, IMapper mapper)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
        }

        public async Task<CourseResponseDto> GetCourseById(int id)
        {
            var courseEntity = await _courseRepository.GetCourseById(id);

            if (courseEntity is null)
            {
                return null;
            }

            var dto = new CourseResponseDto
            {
                CourseID = courseEntity.CourseID,
                CourseName = courseEntity.CourseName,
                Description = courseEntity.Description
            };
            return (dto);
        }

        public async Task<ApiResult<List<CourseResponseDto>>> GetAllCoursesAsync()
        {
            // Lấy toàn bộ entity từ repository
            var courses = await _courseRepository.GetAllCourseAsync();

            // Map sang DTO (dùng LINQ đơn giản; nếu có AutoMapper thì dùng AutoMapper)
            var data = courses.Select(c => new CourseResponseDto
            {
                CourseID = c.CourseID,
                CourseName = c.CourseName,
                Description = c.Description
            }).ToList();

            // Trả về gói kết quả
            return new ApiResult<List<CourseResponseDto>>
            {
                Success = true,
                Message = "Lấy danh sách khóa học thành công",
                Data = data
            };
        }

        public async Task<CourseResponseDto> CreateCourseAsync(CourseRequestDto request, int userId)
        {
            var course = new Course
            {
                UserID = userId,
                CourseName = request.CourseName,
                Description = request.Description,
            };

            var createCourse = await _courseRepository.CreateCourseAsync(course);

            return new CourseResponseDto
            {
                CourseID = createCourse.CourseID,
                CourseName = createCourse.CourseName,
                Description = createCourse.Description
            };
        }

        // Update course
        public async Task<CourseResponseDto> UpdateCourseAsync(int courseId, UpdateCourseDto courseDto)
        {
            var course = await _courseRepository.GetCourseById(courseId);
            if (course == null)
            {
                return null;
            }
            course.CourseName = courseDto.CourseName;
            course.Description = courseDto.Description;

            await _courseRepository.UpdateCourseAsync(course);

            var response = new CourseResponseDto
            {
                CourseID = course.CourseID,
                CourseName = course.CourseName,
                Description = course.Description
            };

            return (response);
        }

        // Delete course
        public async Task<ApiResult<bool>> DeleteCourseById(int courseId)
        {
            var course = await _courseRepository.GetCourseById(courseId);
            if (course == null)
            {
                return new ApiResult<bool>
                {
                    Success = false,
                    Message = $"Can't not find Cousre with {courseId}"
                };
            }
            await _courseRepository.DeleteCourseAsync(course);


            return new ApiResult<bool>
            {
                Success = true,
                Message = "Delete Successful",
                Data = true
            };
        }

        // lấy danh sách event thoe courseId
        public async Task<List<EventDto>> GetListParentEventByCourseId(int courseId)
        {
            var events = await _courseRepository.GetListParentEventByCourseId(courseId);
            var eventDto = _mapper.Map<List<EventDto>>(events);
            return eventDto;
        }

        public async Task<List<CourseResponseDto>> GetCourseByUserId(int userId)
        {
            var course = await _courseRepository.GetCourseByUserId(userId);
            var courseDto = _mapper.Map<List<CourseResponseDto>>(course);
            if (course == null)
            {
                return null;
            }
            return courseDto;
        }
    }
}
