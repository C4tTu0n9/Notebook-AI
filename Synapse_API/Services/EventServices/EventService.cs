using Synapse_API.Repositories;
using Synapse_API.Models.Dto.EventDTOs;
using AutoMapper;
using Synapse_API.Models.Entities;
using Synapse_API.Models.Enums;
using Synapse_API.Repositories.Profile;
using Synapse_API.Repositories.Course;
namespace Synapse_API.Services.EventServices
{
    public class EventService
    {
        private readonly EventRepository _eventRepository;
        private readonly IMapper _mapper;
        private readonly UserProfileRepository _userProfileRepository;
        private readonly CourseRepository _courseRepository;
        private readonly TopicRepository _topicRepository;
        private readonly EventReminderService _reminderService;

        public EventService(EventRepository eventRepository, IMapper mapper, UserProfileRepository userProfileRepository, CourseRepository courseRepository, TopicRepository topicRepository, EventReminderService reminderService)
        {
            _eventRepository = eventRepository;
            _mapper = mapper;
            _userProfileRepository = userProfileRepository;
            _courseRepository = courseRepository;
            _topicRepository = topicRepository;
            _reminderService = reminderService;
        }

        public async Task<EventDto> GetEventById(int id)
        {
            var e = await _eventRepository.GetEventById(id);
            var eDto = _mapper.Map<EventDto>(e);
            return eDto;
        }
        public async Task<IEnumerable<EventDto>> GetEventsByStudentId(int studentId)
        {
            var events = await _eventRepository.GetEventsByStudentId(studentId);
            var eventsDto = _mapper.Map<IEnumerable<EventDto>>(events);
            return eventsDto;
        }
        public async Task<IEnumerable<EventDto>> GetParentEventsByStudentId(int studentId)
        {
            var events = await _eventRepository.GetParentEventsByStudentId(studentId);
            var eventsDto = _mapper.Map<IEnumerable<EventDto>>(events);
            return eventsDto;
        }
        public async Task<EventDto> GetEventAndItsSubEventById(int eventId)
        {
            var events = await _eventRepository.GetEventAndItsSubEventById(eventId);
            var eventsDto = _mapper.Map<EventDto>(events);
            return eventsDto;
        }

        public async Task<IEnumerable<EventDto>> GetAllEvents()
        {
            var events = await _eventRepository.GetAllEvents();
            var eventsDto = _mapper.Map<IEnumerable<EventDto>>(events);
            return eventsDto;
        }
        
        public async Task<EventDto> CreateEvent(CreateEventDto ce)
        {
            var e = _mapper.Map<Event>(ce);
            var eDto = await _eventRepository.CreateEvent(e);
            
            // Tự động tạo reminder mặc định nếu event trong tương lai
            if (e.StartTime > DateTime.Now)
            {
                await _reminderService.CreateDefaultRemindersAsync(eDto.EventID);
            }
            
            return _mapper.Map<EventDto>(eDto);
        }

        public async Task<EventDto> UpdateEvent(UpdateEventDto ue)
        {
            var eventEntity = await _eventRepository.GetEventById(ue.EventID);
            _mapper.Map(ue, eventEntity);
            var updated = await _eventRepository.UpdateEvent(eventEntity);
            return _mapper.Map<EventDto>(updated);
        }

        public async Task UpdateChildEventIsCompleted(int id, bool isCompleted)
        {
            var child = await _eventRepository.GetEventById(id);
            if (child == null)
                throw new Exception("Invalid event.");
            child.IsCompleted = isCompleted;
            await _eventRepository.UpdateEvent(child);
        }


        public async Task<bool> DeleteChileEventsByParentEventId(int parentEventId)
        {
            var subEvents = await _eventRepository.DeleteChildEventsByParentEventId(parentEventId);
            return subEvents;
        }

        public async Task<EventDto> DeleteEvent(int id)
        {
            var e = await _eventRepository.DeleteEvent(id);
            return _mapper.Map<EventDto>(e);
        }

        // Method mới để tạo lịch trình ôn thi tự động
        public async Task<List<StudySessionDto>> GenerateStudyPlan(GenerateStudyPlanDto generateDto)
        {
            // 1. Lấy thông tin exam event
            var examEvent = await _eventRepository.GetEventById(generateDto.ExamEventID);
            if (examEvent == null || examEvent.EventType != EventType.Exam)
            {
                throw new Exception("Event không tồn tại hoặc không phải là EXAM-Typed");
            }

            // 2. Lấy thông tin user profile để biết thời gian rảnh
            var userProfile = await _userProfileRepository.GetUserProfileByUserId(generateDto.UserID);
            
            if (userProfile == null)
            {
                throw new Exception("Không tìm thấy thông tin user profile");
            }

            // 3. Lấy thông tin course và topics
            var course = await _courseRepository.GetCourseById(generateDto.CourseID);
            
            if (course == null)
            {
                throw new Exception("Không tìm thấy thông tin môn học");
            }

            // 4. Xóa các study sessions cũ nếu có
            await _eventRepository.DeleteStudySessionsByParentEvent(generateDto.ExamEventID);

            // 5. Tính toán và tạo lịch trình ôn thi
            var studySessions = new List<Event>();
            var studySessionDtos = new List<StudySessionDto>();
            
            var daysBeforeExam = generateDto.DaysBeforeExam ?? 7;
            var startDate = generateDto.ExamDate.AddDays(-daysBeforeExam);
            var topics = course.Topics.ToList();
            var topicsPerDay = Math.Max(1, topics.Count / daysBeforeExam);
            
            // Lấy thời gian học mỗi ngày từ user profile (mặc định 2 giờ)
            var dailyStudyHours = userProfile.DailyStudyHours ?? 2;
            var preferredTime = userProfile.PreferredStudyTime ?? "Evening";
            
            var topicIndex = 0;
            for (int day = 0; day < daysBeforeExam && topicIndex < topics.Count; day++)
            {
                var studyDate = startDate.AddDays(day);
                
                // Xác định giờ học dựa trên preference
                var startHour = preferredTime switch
                {
                    "Morning" => 8,
                    "Afternoon" => 14,
                    "Evening" => 19,
                    _ => 19
                };

                // Kiểm tra xem có event nào khác trong ngày không
                var existingEvents = await _eventRepository.GetEventsByUserAndDateRange(
                    generateDto.UserID, 
                    studyDate.Date, 
                    studyDate.Date.AddDays(1)
                );

                // Điều chỉnh giờ học nếu có xung đột
                while (existingEvents.Any(e => 
                    e.StartTime.Hour <= startHour + dailyStudyHours && 
                    e.EndTime.Hour >= startHour))
                {
                    startHour += 2;
                    if (startHour >= 22) // Nếu quá muộn thì chuyển sang buổi sáng
                    {
                        startHour = 6;
                        break;
                    }
                }

                // Tạo study session cho ngày này
                var topicsForToday = new List<Topic>();
                for (int i = 0; i < topicsPerDay && topicIndex < topics.Count; i++)
                {
                    topicsForToday.Add(topics[topicIndex]);
                    topicIndex++;
                }

                var hoursPerTopic = (double)dailyStudyHours / topicsForToday.Count;
                var currentStartTime = studyDate.Date.AddHours(startHour);

                foreach (var topic in topicsForToday)
                {
                    var studySession = new Event
                    {
                        UserID = generateDto.UserID,
                        CourseID = generateDto.CourseID,
                        ParentEventID = generateDto.ExamEventID,
                        EventType = EventType.StudySession,
                        Title = $"Practice {topic.TopicName}",
                        Description = $"Practice {topic.TopicName} for exam {examEvent.Title}. " +
                                    $"Content: {topic.Description ?? ""}",
                        StartTime = currentStartTime,
                        EndTime = currentStartTime.AddHours(hoursPerTopic),
                        Priority = 1, // Mức độ ưu tiên trung bình
                        IsCompleted = false
                    };

                    studySessions.Add(studySession);
                    
                    // Tạo DTO để trả về
                    var sessionDto = new StudySessionDto
                    {
                        Title = studySession.Title,
                        Description = studySession.Description,
                        StartTime = studySession.StartTime,
                        EndTime = studySession.EndTime,
                        TopicID = topic.TopicID,
                        TopicName = topic.TopicName,
                        EstimatedDuration = (int)(hoursPerTopic * 60), // chuyển sang phút
                        DayNumber = day + 1
                    };
                    
                    studySessionDtos.Add(sessionDto);
                    currentStartTime = currentStartTime.AddHours(hoursPerTopic);
                }
            }

            // 6. Lưu các study sessions vào database
            var createdEvents = await _eventRepository.CreateMultipleEvents(studySessions);
            
            // Update EventID cho DTOs
            for (int i = 0; i < studySessionDtos.Count && i < createdEvents.Count(); i++)
            {
                studySessionDtos[i].EventID = createdEvents.ElementAt(i).EventID;
            }

            return studySessionDtos;
        }

        // Method để lấy lịch trình ôn thi của một exam
        public async Task<IEnumerable<StudySessionDto>> GetStudyPlanByExamId(int examId)
        {
            var studySessions = await _eventRepository.GetStudySessionsByParentEvent(examId);
            
            var result = new List<StudySessionDto>();
            foreach (var session in studySessions)
            {
                // Lấy thông tin topic nếu có
                var topic = session.CourseID.HasValue ? 
                    await _topicRepository.GetTopicById(session.CourseID) : null;
                
                var dto = new StudySessionDto
                {
                    EventID = session.EventID,
                    Title = session.Title,
                    Description = session.Description,
                    StartTime = session.StartTime,
                    EndTime = session.EndTime,
                    TopicID = topic?.TopicID ?? 0,
                    TopicName = topic?.TopicName ?? "",
                    EstimatedDuration = (int)(session.EndTime - session.StartTime).TotalMinutes,
                    DayNumber = (session.StartTime.Date - studySessions.Min(s => s.StartTime.Date)).Days + 1
                };
                
                result.Add(dto);
            }
            
            return result.OrderBy(r => r.StartTime);
        }
    }
}
