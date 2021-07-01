using ModalLayer.Modal;
using System.Data;

namespace ServiceLayer.Interface
{
    public interface IManageUserCommentService<T>
    {
        string PostUserCommentService(UserComments userComments);
        DataSet GetCommentsService(string EmailId);
    }
}
