using System;
using System.Collections.Generic;
using System.Linq;
using positioning_stronghold.Models;

namespace positioning_stronghold
{
    // 三角测量计算器类
    // 用于根据多个测量点的角度和坐标计算目标位置
    // 支持两点交点法和多点最小二乘法
    public class TriangulationCalculator
    {
        // 计算结果类
        public class CalculationResult
        {
            // 计算得到的X坐标
            public double X { get; set; }
            
            // 计算得到的Z坐标
            public double Z { get; set; }
            
            // 计算误差（角度偏差）
            public double Error { get; set; }
            
            // 计算是否成功
            public bool Success { get; set; }
            
            // 错误或状态消息
            public string Message { get; set; }

            // 默认构造函数
            public CalculationResult()
            {
                Success = false;
            }

            // 成功结果构造函数
            public CalculationResult(double x, double z, double error)
            {
                X = x;
                Z = z;
                Error = error;
                Success = true;
            }
        }

        // 计算目标位置
        public static CalculationResult CalculatePosition(List<MeasurementPoint> points)
        {
            if (points == null || points.Count < 2)
            {
                return new CalculationResult { Message = "至少需要2个测量点" };
            }

            if (points.Count == 2)
            {
                return CalculateTwoPoints(points[0], points[1]);
            }

            return CalculateLeastSquares(points);
        }

        // 使用两点交点法计算位置
        private static CalculationResult CalculateTwoPoints(MeasurementPoint p1, MeasurementPoint p2)
        {
            double yaw1Rad = p1.Yaw * Math.PI / 180.0;
            double yaw2Rad = p2.Yaw * Math.PI / 180.0;

            double dx1 = -Math.Sin(yaw1Rad);
            double dz1 = Math.Cos(yaw1Rad);
            double dx2 = -Math.Sin(yaw2Rad);
            double dz2 = Math.Cos(yaw2Rad);

            double denominator = dx1 * dz2 - dx2 * dz1;

            if (Math.Abs(denominator) < 0.0001)
            {
                return new CalculationResult { Message = "两条射线平行，无法计算交点" };
            }

            double t = ((p2.X - p1.X) * dz2 - (p2.Z - p1.Z) * dx2) / denominator;

            double x = p1.X + dx1 * t;
            double z = p1.Z + dz1 * t;

            double error = CalculateError(new List<MeasurementPoint> { p1, p2 }, x, z);

            return new CalculationResult(x, z, error);
        }

        // 使用最小二乘法计算位置
        private static CalculationResult CalculateLeastSquares(List<MeasurementPoint> points)
        {
            int n = points.Count;
            double sumA = 0, sumB = 0, sumC = 0, sumD = 0;
            double sumE = 0;

            foreach (var p in points)
            {
                double yawRad = p.Yaw * Math.PI / 180.0;
                double dx = -Math.Sin(yawRad);
                double dz = Math.Cos(yawRad);

                double a = dx * dx;
                double b = dx * dz;
                double c = dz * dz;
                double d = p.X * dx + p.Z * dz;

                sumA += a;
                sumB += b;
                sumC += c;
                sumD += d;
            }

            double denominator = sumA * sumC - sumB * sumB;

            if (Math.Abs(denominator) < 0.0001)
            {
                return new CalculationResult { Message = "无法计算唯一解" };
            }

            double x = (sumC * sumD - sumB * sumE) / denominator;
            double z = (sumA * sumE - sumB * sumD) / denominator;

            double error = CalculateError(points, x, z);

            return new CalculationResult(x, z, error);
        }

        // 计算角度误差
        private static double CalculateError(List<MeasurementPoint> points, double x, double z)
        {
            double totalError = 0;

            foreach (var p in points)
            {
                double yawRad = p.Yaw * Math.PI / 180.0;
                double dx = -Math.Sin(yawRad);
                double dz = Math.Cos(yawRad);

                double expectedX = p.X + dx;
                double expectedZ = p.Z + dz;

                double dirX = x - p.X;
                double dirZ = z - p.Z;

                double distance = Math.Sqrt(dirX * dirX + dirZ * dirZ);
                if (distance > 0)
                {
                    dirX /= distance;
                    dirZ /= distance;
                }

                double dot = dx * dirX + dz * dirZ;
                double angle = Math.Acos(Math.Max(-1, Math.Min(1, dot)));
                double angleDeg = angle * 180.0 / Math.PI;

                totalError += angleDeg;
            }

            return totalError / points.Count;
        }

        // 计算两点之间的方向角度
        public static double GetDirectionAngle(double fromX, double fromZ, double toX, double toZ)
        {
            double dx = toX - fromX;
            double dz = toZ - fromZ;

            double angle = Math.Atan2(dx, dz) * 180.0 / Math.PI;

            angle = angle - 90;

            if (angle < -180)
                angle += 360;

            if (angle > 180)
                angle -= 360;

            return angle;
        }
    }
}