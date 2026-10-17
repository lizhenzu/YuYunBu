using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models;
using BulletinBoard.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;
using System.Data.SqlClient;

namespace BulletinBoard.BusinessLayer
{
     

    public class BoardRepository
    {
        
        public BoardRepository()
        { 
        }
        public void AddBoard(string boardName, Board board)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(DataAccessModule.connectionString_Server))
                {
                    conn.Open();
                    var query = $"INSERT INTO Board{boardName} (Title, Content, UserName, CreatedDate) VALUES (@Title, @Content, @UserName, DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE()))";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Title", board.Title);
                        cmd.Parameters.AddWithValue("@Content", board.Content);
                        if (string.IsNullOrEmpty(board.UserName))
                        {
                            board.UserName = string.Empty;
                        }
                        if (boardName.ToLower() == BoardCategory.Anonymous.ToString().ToLower())
                        {
                            cmd.Parameters.AddWithValue("@UserName", string.Empty);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@UserName", board.UserName);
                        }
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
             
        }

        public void DeleteBoard(string boardName, int id)
        {
            using (SqlConnection conn = new SqlConnection(DataAccessModule.connectionString_Server))
            {
                conn.Open();
              //  var query = $" delete from Board{boardName}  where id = @Id ";
                var query = $"EXEC DeleteBoardWithComments @masterTable = 'Board{boardName}', @slateTable = 'Comment{boardName}', @BoardId = @Id; ";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void HitBoard(string boardName, int id)
        {
            using (SqlConnection conn = new SqlConnection(DataAccessModule.connectionString_Server))
            {
                conn.Open();
                var query = $"update   Board{boardName} set [visited] = [visited] + 1 where id = @Id ";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<Board> GetBoardList(string boardName)
        {
            var boards = new List<Board>();
            using (SqlConnection conn = new SqlConnection(DataAccessModule.connectionString_Server))
            {
                conn.Open();
                var query = $"SELECT Id, Title, Content, visited, isnull([UserName],'') UserName, CreatedDate  FROM Board{boardName} order by CreatedDate desc";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            boards.Add(new Board
                            {
                                Id = reader.GetInt32(0),
                                Title = reader.GetString(1),
                                Content = reader.GetString(2),
                                Visited = reader.GetInt32(3),
                                UserName = reader.GetString(4),
                                CreatedDate = reader.GetDateTime(5)
                            });
                        }
                    }
                }
            }
            return boards;
        }

        public Board GetBoardById(string boardName, int id)
        {
            Board board = null;
            using (SqlConnection conn = new SqlConnection(DataAccessModule.connectionString_Server))
            {
                conn.Open();
                var query = $"SELECT Id, Title, Content, visited, isnull([UserName],'') UserName, CreatedDate FROM Board{boardName} WHERE Id = @Id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            board = new Board
                            {
                                Id = reader.GetInt32(0),
                                Title = reader.GetString(1),
                                Content = reader.GetString(2),
                                Visited = reader.GetInt32(3),
                                UserName = reader.GetString(4),
                                CreatedDate = reader.GetDateTime(5)
                            };
                        }
                    }
                }
            }
            return board;
        }
        

        public List<Comment> GetCommentsByBoardId(string boardName, int boardId)
        {
            var comments = new List<Comment>();
            var commentDictionary = new Dictionary<int, Comment>();

            using (SqlConnection conn = new SqlConnection(DataAccessModule.connectionString_Server))
            {
                conn.Open();
                var query = $"SELECT Id, BoardId, ParentCommentId, Content, isnull([UserName],'') UserName, CreatedDate FROM Comment{boardName} WHERE BoardId = @BoardId ORDER BY CreatedDate";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@BoardId", boardId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var comment = new Comment
                            {
                                Id = reader.GetInt32(0),
                                BoardId = reader.GetInt32(1),
                                ParentCommentId = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2),
                                Content = reader.GetString(3),
                                UserName = reader.GetString(4),
                                CreatedDate = reader.GetDateTime(5),
                                ChildComments = new List<Comment>() // 자식 댓글을 추가하기 위한 리스트 초기화
                            };

                            // 딕셔너리에 추가
                            commentDictionary[comment.Id] = comment;

                            // 부모 댓글이 없으면 최상위 댓글로 처리
                            if (!comment.ParentCommentId.HasValue)
                            {
                                comments.Add(comment);
                            }
                            else
                            {
                                // 부모 댓글이 있으면 부모의 ChildComments에 추가
                                if (commentDictionary.ContainsKey(comment.ParentCommentId.Value))
                                {
                                    commentDictionary[comment.ParentCommentId.Value].ChildComments.Add(comment);
                                }
                            }
                        }
                    }
                }
            }
            return comments;
        }

        public void AddComment(string boardName, int boardId, string content, string UserName, int? parentCommentId = null)
        {
            using (SqlConnection conn = new SqlConnection(DataAccessModule.connectionString_Server))
            {
                conn.Open();
                var query = $"INSERT INTO Comment{boardName} (BoardId, ParentCommentId, Content, UserName, CreatedDate) VALUES (@BoardId, @ParentCommentId, @Content, @UserName, DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE()))";
              
                if (string.IsNullOrEmpty(UserName))
                {
                    UserName = string.Empty;
                }
                
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@BoardId", boardId);
                    cmd.Parameters.AddWithValue("@ParentCommentId", parentCommentId.HasValue ? (object)parentCommentId.Value : DBNull.Value);
                    if (boardName.ToLower() == BoardCategory.Anonymous.ToString().ToLower())
                    {
                        cmd.Parameters.AddWithValue("@UserName", string.Empty);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@UserName", UserName);
                    }
                    cmd.Parameters.AddWithValue("@Content", content);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
