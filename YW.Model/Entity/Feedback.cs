using System; 
using System.Text;
using System.Collections.Generic; 
using System.Data;
namespace YW.Model.Entity{
	 	//Feedback
	[Serializable]
	public class Feedback
	{
      	private int _feedbackid;
		/// <summary>
		/// FeedbackID
        /// </summary>		
        public int FeedbackID
        {
            get{ return _feedbackid; }
            set{ _feedbackid = value; }
        }        
		private int _questiontype;
		/// <summary>
		/// 问答类型：0表示APP，1表示设备
        /// </summary>		
        public int QuestionType
        {
            get{ return _questiontype; }
            set{ _questiontype = value; }
        }        
		private int _questionuserid;
		/// <summary>
		/// 提问用户
        /// </summary>		
        public int QuestionUserID
        {
            get{ return _questionuserid; }
            set{ _questionuserid = value; }
        }        
		private string _questionimg;
		/// <summary>
		/// 问题截图
        /// </summary>		
        public string QuestionImg
        {
            get{ return _questionimg; }
            set{ _questionimg = value; }
        }        
		private string _questioncontent;
		/// <summary>
		/// 问题内容
        /// </summary>		
        public string QuestionContent
        {
            get{ return _questioncontent; }
            set{ _questioncontent = value; }
        }        
		private int _answeruserid;
		/// <summary>
		/// 回答用户ID，一般为系统超级管理员
        /// </summary>		
        public int AnswerUserID
        {
            get{ return _answeruserid; }
            set{ _answeruserid = value; }
        }        
		private string _answercontent;
		/// <summary>
		/// AnswerContent
        /// </summary>		
        public string AnswerContent
        {
            get{ return _answercontent; }
            set{ _answercontent = value; }
        }        
		private int _feedbackstate;
		/// <summary>
		/// 反馈状态：0表示提问，1表示回答，2表示被甄选为经典问答解疑内容
        /// </summary>		
        public int FeedbackState
        {
            get{ return _feedbackstate; }
            set{ _feedbackstate = value; }
        }        
		private DateTime _createtime;
		/// <summary>
		/// CreateTime
        /// </summary>		
        public DateTime CreateTime
        {
            get{ return _createtime; }
            set{ _createtime = value; }
        }        
		private DateTime? _handletime;
		/// <summary>
		/// HandleTime
        /// </summary>		
        public DateTime? HandleTime
        {
            get{ return _handletime; }
            set{ _handletime = value; }
        }        
		private int _handleuserid;
		/// <summary>
		/// HandleUserID
        /// </summary>		
        public int HandleUserID
        {
            get{ return _handleuserid; }
            set{ _handleuserid = value; }
        }        
		private bool _deleted;
		/// <summary>
		/// Deleted
        /// </summary>		
        public bool Deleted
        {
            get{ return _deleted; }
            set{ _deleted = value; }
        }        
		   
	}
}

