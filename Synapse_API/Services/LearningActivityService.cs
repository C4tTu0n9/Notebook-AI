using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Core.Types;
using Synapse_API.Models.Dto.LearningAnalysisDTOs;
using Synapse_API.Models.Entities;
using Synapse_API.Repositories;

namespace Synapse_API.Services
{
    public class LearningActivityService
    {
        private readonly LearningActivityRepository _learningActivityRepository;

        public LearningActivityService(LearningActivityRepository learningActivityRepository)
        {
            _learningActivityRepository = learningActivityRepository;
        }

        public async Task<int> RecordLearningActivityAsync(int userId, LearningActivityDto dto)
        {
            if (dto.StartTime >= dto.EndTime)
                throw new ArgumentException("EndTime must be after StartTime.");

            int duration = (int)(dto.EndTime - dto.StartTime).TotalMinutes;

            var activity = new LearningActivity
            {
                UserID = userId,
                ActivityType = dto.ActivityType,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Duration = duration
            };

            await _learningActivityRepository.AddAsync(activity);
            return activity.ActivityID;
        }


        public async Task<List<LearningActivity>> GetAllLearningActivity()
        {
            var activities = await _learningActivityRepository.GetAllLearningActivity();

            return activities;
        }


    }
}
