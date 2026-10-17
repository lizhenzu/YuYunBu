using BulletinBoard.Common;
using Microsoft.Extensions.Logging;
using BulletinBoard.Common;
using System;
using System.Data; 
using System.Reflection;
using System.Text;

namespace BulletinBoard.DataAccessLayer
{
    public class ControlEXERCISE
    {
        public DataSet fetchExerciseDetails(int exerciseId)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("DECLARE @ExerciseID INT;");
            sb.AppendFormat("SET @ExerciseID = {0};", exerciseId);
            sb.AppendLine();

            // 마스터 운동 정보
            sb.AppendLine("SELECT * FROM EXERCISE WHERE ExerciseID = @ExerciseID;");
            sb.AppendLine();

            // 자세 및 동작
            sb.AppendLine("SELECT * FROM EXERCISE_STEP WHERE ExerciseID = @ExerciseID ORDER BY StepOrder;");
            sb.AppendLine();

            // 운동 부위
            sb.AppendLine("SELECT * FROM EXERCISE_MUSCLE WHERE ExerciseID = @ExerciseID;");
            sb.AppendLine();

            // 운동 효과
            sb.AppendLine("SELECT * FROM EXERCISE_BENEFIT WHERE ExerciseID = @ExerciseID;");
            sb.AppendLine();

            // 어린이용 설명
            sb.AppendLine("SELECT * FROM EXERCISE_KIDS WHERE ExerciseID = @ExerciseID;");

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }


    }
}
