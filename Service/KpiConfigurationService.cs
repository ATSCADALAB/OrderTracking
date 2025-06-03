using AutoMapper;
using Contracts;
using Entities.Models;
using Service.Contracts;
using Shared.DataTransferObjects.KpiConfiguration;

namespace Service
{
    internal sealed class KpiConfigurationService : IKpiConfigurationService
    {
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;

        public KpiConfigurationService(IRepositoryManager repository, ILoggerManager logger, IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<KpiConfigurationDto?> GetActiveConfigurationAsync()
        {
            var config = await _repository.KpiConfiguration.GetActiveConfigurationAsync(false);
            return config == null ? null : _mapper.Map<KpiConfigurationDto>(config);
        }

        public async Task<KpiConfigurationDto?> GetConfigurationByIdAsync(int id)
        {
            var config = await _repository.KpiConfiguration.GetConfigurationByIdAsync(id, false);
            return config == null ? null : _mapper.Map<KpiConfigurationDto>(config);
        }

        public async Task<IEnumerable<KpiConfigurationDto>> GetAllConfigurationsAsync()
        {
            var configs = await _repository.KpiConfiguration.GetAllConfigurationsAsync(false);
            return _mapper.Map<IEnumerable<KpiConfigurationDto>>(configs);
        }

        public async Task<KpiConfigurationDto> CreateConfigurationAsync(KpiConfigurationForCreationDto configDto)
        {
            // Deactivate current active configuration
            var currentActive = await _repository.KpiConfiguration.GetActiveConfigurationAsync(true);
            if (currentActive != null)
            {
                currentActive.IsActive = false;
                currentActive.UpdatedAt = DateTime.UtcNow;
                _repository.KpiConfiguration.UpdateConfiguration(currentActive);
            }

            var config = _mapper.Map<KpiConfiguration>(configDto);
            config.CreatedAt = DateTime.UtcNow;
            config.IsActive = true;

            _repository.KpiConfiguration.CreateConfiguration(config);
            await _repository.SaveAsync();

            return _mapper.Map<KpiConfigurationDto>(config);
        }

        public async Task UpdateConfigurationAsync(int id, KpiConfigurationForUpdateDto configDto)
        {
            var config = await _repository.KpiConfiguration.GetConfigurationByIdAsync(id, true);
            if (config == null)
                throw new Exception($"KPI Configuration with id '{id}' not found");

            _mapper.Map(configDto, config);
            config.UpdatedAt = DateTime.UtcNow;

            _repository.KpiConfiguration.UpdateConfiguration(config);
            await _repository.SaveAsync();
        }

        public async Task DeleteConfigurationAsync(int id)
        {
            var config = await _repository.KpiConfiguration.GetConfigurationByIdAsync(id, true);
            if (config == null)
                throw new Exception($"KPI Configuration with id '{id}' not found");

            _repository.KpiConfiguration.DeleteConfiguration(config);
            await _repository.SaveAsync();
        }

        public async Task<KpiConfigurationDto> SetActiveConfigurationAsync(int id)
        {
            // Deactivate all configurations
            var allConfigs = await _repository.KpiConfiguration.GetAllConfigurationsAsync(true);
            foreach (var config in allConfigs)
            {
                config.IsActive = false;
                config.UpdatedAt = DateTime.UtcNow;
                _repository.KpiConfiguration.UpdateConfiguration(config);
            }

            // Activate the specified configuration
            var targetConfig = await _repository.KpiConfiguration.GetConfigurationByIdAsync(id, true);
            if (targetConfig == null)
                throw new Exception($"KPI Configuration with id '{id}' not found");

            targetConfig.IsActive = true;
            targetConfig.UpdatedAt = DateTime.UtcNow;
            _repository.KpiConfiguration.UpdateConfiguration(targetConfig);

            await _repository.SaveAsync();

            return _mapper.Map<KpiConfigurationDto>(targetConfig);
        }

        public async Task<KpiConfigurationDto> CreateDefaultConfigurationAsync()
        {
            var defaultConfig = new KpiConfigurationForCreationDto
            {
                Description = "Default KPI Configuration - Auto Generated"
            };

            return await CreateConfigurationAsync(defaultConfig);
        }

        // Helper methods for KPI calculation
        public async Task<int> CalculateStarsAsync(int daysLate)
        {
            var config = await _repository.KpiConfiguration.GetActiveConfigurationAsync(false);
            if (config == null)
                throw new Exception("No active KPI configuration found");

            return daysLate switch
            {
                < 0 => config.Stars_EarlyCompletion,  // Hoàn thành sớm
                0 => config.Stars_OnTime,             // Đúng hạn
                1 => config.Stars_Late1Day,           // Trễ 1 ngày
                2 => config.Stars_Late2Days,          // Trễ 2 ngày
                _ => config.Stars_Late3OrMoreDays     // Trễ 3+ ngày
            };
        }

        public async Task<(int lightOrders, int mediumOrders, int heavyOrders)> CategorizeOrdersAsync(IEnumerable<int> orderDays)
        {
            var config = await _repository.KpiConfiguration.GetActiveConfigurationAsync(false);
            if (config == null)
                throw new Exception("No active KPI configuration found");

            var lightOrders = orderDays.Count(days => days < config.LightOrder_MaxDays);
            var mediumOrders = orderDays.Count(days => days >= config.MediumOrder_MinDays && days <= config.MediumOrder_MaxDays);
            var heavyOrders = orderDays.Count(days => days >= config.HeavyOrder_MinDays);

            return (lightOrders, mediumOrders, heavyOrders);
        }

        public async Task<decimal> CalculateHSSLAsync(int lightOrders, int mediumOrders, int heavyOrders)
        {
            var config = await _repository.KpiConfiguration.GetActiveConfigurationAsync(false);
            if (config == null)
                throw new Exception("No active KPI configuration found");

            // HSSL = [MAX(SLn – 5, 0)]*0.1 + SLv*1 + SLl * 2
            var excessLightOrders = Math.Max(lightOrders - config.HSSL_LightOrderFreeCount, 0);
            var hssl = excessLightOrders * config.HSSL_LightOrderMultiplier +
                      mediumOrders * config.HSSL_MediumOrderMultiplier +
                      heavyOrders * config.HSSL_HeavyOrderMultiplier;

            return hssl;
        }

        public async Task<decimal> CalculateRewardAsync(decimal averageStars, decimal hssl, decimal penalty)
        {
            var config = await _repository.KpiConfiguration.GetActiveConfigurationAsync(false);
            if (config == null)
                throw new Exception("No active KPI configuration found");

            decimal reward = 0;

            if (averageStars >= config.Reward_HighPerformance_MinStars && averageStars <= config.Reward_HighPerformance_MaxStars)
            {
                // Công thức thưởng: (TD-3)*2500000/2-500000 – PB * HSSL
                reward = ((averageStars - config.Penalty_MediumPerformance_MinStars) * config.Reward_BaseAmount / 2 - config.Reward_BasePenalty - penalty) * hssl;
            }
            else if (averageStars >= config.Penalty_MediumPerformance_MinStars && averageStars < config.Penalty_MediumPerformance_MaxStars)
            {
                // Công thức phạt nhẹ: (TD-3)*2500000/2-500000 + PB
                reward = (averageStars - config.Penalty_MediumPerformance_MinStars) * config.Reward_BaseAmount / 2 - config.Reward_BasePenalty + penalty;
            }
            else if (averageStars >= config.Penalty_LowPerformance_MinStars && averageStars < config.Penalty_LowPerformance_MaxStars)
            {
                // Công thức phạt nặng: (TD-1)*500000/2-1000000 + PB
                reward = (averageStars - config.Penalty_LowPerformance_MinStars) * config.Penalty_LowPerformance_BaseAmount / 2 - config.Penalty_LowPerformance_MaxPenalty + penalty;
            }

            return reward;
        }
    }
}