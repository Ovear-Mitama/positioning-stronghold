using System;
using System.Text.RegularExpressions;
using positioning_stronghold.Models;

namespace positioning_stronghold
{
    // 命令解析器类，用于解析游戏命令字符串
    public class CommandParser
    {
        // 解析命令字符串，提取坐标和角度信息
        // 支持格式：/execute in [dimension] run tp @s [x] [y] [z] [yaw] [pitch]
        public static MeasurementPoint ParseCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return null;

            var match = Regex.Match(command, @"/execute\s+in\s+(\S+)\s+run\s+tp\s+@s\s+(-?\d+\.?\d*)\s+(-?\d+\.?\d*)\s+(-?\d+\.?\d*)\s+(-?\d+\.?\d*)\s+(-?\d+\.?\d*)", RegexOptions.IgnoreCase);
            if (!match.Success)
                return null;

            try
            {
                string dimension = match.Groups[1].Value;
                double x = double.Parse(match.Groups[2].Value);
                double y = double.Parse(match.Groups[3].Value);
                double z = double.Parse(match.Groups[4].Value);
                double yaw = double.Parse(match.Groups[5].Value);
                double pitch = double.Parse(match.Groups[6].Value);

                yaw = NormalizeAngle(yaw);

                return new MeasurementPoint(0, x, y, z, yaw, pitch, dimension);
            }
            catch
            {
                return null;
            }
        }

        // 验证俯仰角是否在有效范围内
        public static bool IsValidAngle(double pitch)
        {
            return pitch >= -90 && pitch <= 0;
        }

        // 将角度标准化到-180到180度之间
        private static double NormalizeAngle(double angle)
        {
            angle = angle % 360;
            if (angle > 180)
            {
                angle -= 360;
            }
            else if (angle < -180)
            {
                angle += 360;
            }
            return angle;
        }
    }
}