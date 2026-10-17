using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models;
using BulletinBoard.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;
using System.Data.SqlClient;
using System.Reflection;
using BulletinBoard.Models;

namespace BulletinBoard.BusinessLayer
{

    public class ExerciseService
    {
        /* 데이터 접근 모듈 (앞서 작성한 fetchExerciseDetails 포함) */
        private readonly ControlEXERCISE _repo = new ControlEXERCISE();

        /// <summary>
        /// ExerciseID 로 모든 세부정보를 조회하여 DTO 로 반환
        /// </summary>
        public ExerciseDetail GetExerciseDetail(int exerciseId)
        {
            DataSet ds = _repo.fetchExerciseDetails(exerciseId);

            var detail = new ExerciseDetail
            {
                Basic = ds.Tables[0].ToList<Exercise>(),
                Steps = ds.Tables[1].ToList<ExerciseStep>(),
                Muscles = ds.Tables[2].ToList<ExerciseMuscle>(),
                Benefits = ds.Tables[3].ToList<ExerciseBenefit>(),
                KidsInfo = ds.Tables[4].ToList<ExerciseKids>()
            };

            return detail;
        }

    }
}
