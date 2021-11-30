using System;
using System.Linq;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Volo.Abp.AspNetCore.ExceptionHandling;
using Volo.Abp.Autofac;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.Swashbuckle;
using Volo.Abp.UI.Navigation.Urls;

namespace Lion.AbpPro
{
    [DependsOn(
        typeof(AbpSwashbuckleModule),
        typeof(AbpAutofacModule))]
    public class SharedHostingMicroserviceModule : AbpModule
    {
        private const string DefaultCorsPolicyName = "Default";

        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var configuration = context.Services.GetConfiguration();
            context.Services.AddConsulConfig(configuration);
            ConfigureHealthChecks(context);
            ConfigureLocalization();
            ConfigureCors(context);
            ConfigureUrls(configuration);
            ConfigureAbpExceptions(context);
            ConfigureConsul(context, configuration);
        }

  

        private void ConfigureConsul(ServiceConfigurationContext context,
            IConfiguration configuration)
        {
            if (configuration.GetValue<bool>("Consul:Enabled", false))
            {
                context.Services.AddConsulConfig(configuration);
            }
        }

        /// <summary>
        /// 异常处理
        /// </summary>
        /// <param name="context"></param>
        private void ConfigureAbpExceptions(ServiceConfigurationContext context)
        {
            //开启后通过ErrorCode抛本地化异常，message不会显示本地化词条
            var SendExceptionsDetails = context.Services.GetHostingEnvironment().IsDevelopment();
            context.Services.Configure<AbpExceptionHandlingOptions>(options =>
            {
                options.SendExceptionsDetailsToClients = SendExceptionsDetails;
            });
            context.Services.AddMvc(options =>
            {
                options.Filters.Add(typeof(ResultExceptionFilter));
            });
        }

        /// <summary>
        /// 配置跨域
        /// </summary>
        private void ConfigureCors(ServiceConfigurationContext context)
        {
            var configuration = context.Services.GetConfiguration();
            context.Services.AddCors(options =>
            {
                options.AddPolicy(DefaultCorsPolicyName, builder =>
                {
                    builder
                        .WithOrigins(
                            configuration["App:CorsOrigins"]
                                .Split(",", StringSplitOptions.RemoveEmptyEntries)
                                .Select(o => o.RemovePostFix("/"))
                                .ToArray()
                        )
                        .WithAbpExposedHeaders()
                        .SetIsOriginAllowedToAllowWildcardSubdomains()
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });
        }


        /// <summary>
        /// 站点配置
        /// </summary>
        /// <param name="configuration"></param>
        private void ConfigureUrls(IConfiguration configuration)
        {
            Configure<AppUrlOptions>(options =>
            {
                options.Applications["MVC"].RootUrl = configuration["App:SelfUrl"];
            });
        }

        /// <summary>
        /// 多语言配置
        /// </summary>
        private void ConfigureLocalization()
        {
            Configure<AbpLocalizationOptions>(options =>
            {
                options.Languages.Add(new LanguageInfo("ar", "ar", "العربية"));
                options.Languages.Add(new LanguageInfo("cs", "cs", "Čeština"));
                options.Languages.Add(new LanguageInfo("en", "en", "English"));
                options.Languages.Add(new LanguageInfo("en-GB", "en-GB", "English (UK)"));
                options.Languages.Add(new LanguageInfo("fr", "fr", "Français"));
                options.Languages.Add(new LanguageInfo("hu", "hu", "Magyar"));
                options.Languages.Add(new LanguageInfo("pt-BR", "pt-BR", "Português"));
                options.Languages.Add(new LanguageInfo("ru", "ru", "Русский"));
                options.Languages.Add(new LanguageInfo("tr", "tr", "Türkçe"));
                options.Languages.Add(new LanguageInfo("zh-Hans", "zh-Hans", "简体中文"));
                options.Languages.Add(new LanguageInfo("zh-Hant", "zh-Hant", "繁體中文"));
                options.Languages.Add(new LanguageInfo("de-DE", "de-DE", "Deutsch", "de"));
                options.Languages.Add(new LanguageInfo("es", "es", "Español", "es"));
            });
        }

        /// <summary>
        /// 健康检查
        /// </summary>
        /// <param name="context"></param>
        private void ConfigureHealthChecks(ServiceConfigurationContext context)
        {
            // TODO 检查数据库和redis是否正常 AspNetCore.HealthChecks.Redis AspNetCore.HealthChecks.MySql
            // context.Services.AddHealthChecks().AddRedis(redisConnectionString).AddMySql(connectString);
            context.Services.AddHealthChecks();
        }
    }
}