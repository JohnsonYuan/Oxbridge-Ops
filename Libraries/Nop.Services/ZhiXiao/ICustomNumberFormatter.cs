using Nop.Core.Domain.ZhiXiao;

namespace Nop.Services.ZhiXiao
{
    public interface ICustomNumberFormatter
    {
        /// <summary>
        /// 小组编号码
        /// </summary>
        /// <param name="team">小组</param>
        /// <returns>小组编号码</returns>
        string GenerateTeamNumber(CustomerTeam team);

        /// <summary>
        /// 注册码
        /// </summary>
        /// <returns>注册码</returns>
        string GenerateRegistionCode();
    }
}
