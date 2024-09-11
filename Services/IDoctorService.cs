using Alzaheimer.Models;

namespace Alzaheimer.Services
{
    public interface IDoctorService
    {
        Task<ClinicGet> UpdateClinicAsync(ClinicDto models);
        Task<ScheduleDto> UpdateSchedulAsync(ScheduleDto schedules);
        Task<DetectionDto> DetectionAsync(DetectionDto model);
    }
}
