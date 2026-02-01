using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace positioning_stronghold.Models
{
    // 测量点数据模型类
    // 存储三维坐标、角度和维度信息
    public class MeasurementPoint
    {
        // 测量点唯一标识符
        public int Id { get; set; }
        
        // X坐标（东西方向）
        public double X { get; set; }
        
        // Y坐标（垂直方向）
        public double Y { get; set; }
        
        // Z坐标（南北方向）
        public double Z { get; set; }
        
        // 偏航角（水平方向角度）
        public double Yaw { get; set; }
        
        // 俯仰角（垂直方向角度）
        public double Pitch { get; set; }

        // 维度名称（如minecraft:overworld）
        public string Dimension { get; set; }

        // 默认构造函数
        public MeasurementPoint()
        {
        }

        // 完整构造函数
        public MeasurementPoint(int id, double x, double y, double z, double yaw, double pitch, string dimension = "minecraft:overworld")
        {
            Id = id;
            X = x;
            Y = y;
            Z = z;
            Yaw = yaw;
            Pitch = pitch;
            Dimension = dimension;
        }

        // 格式化显示的X坐标（保留两位小数）
        public string DisplayX => X.ToString("F2");
        
        // 格式化显示的Z坐标（保留两位小数）
        public string DisplayZ => Z.ToString("F2");
        
        // 格式化显示的偏航角（保留两位小数）
        public string DisplayYaw => Yaw.ToString("F2");
    }
}