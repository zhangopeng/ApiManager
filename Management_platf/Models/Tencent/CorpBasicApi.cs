using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Management_platf.Models
{
    public class CorpBasicApi
    {
        public bool CheckSignature(string token, string signature, string timestamp, string nonce, string corpId, string encodingAESKey, string echostr, ref string retEchostr)
        {
            WXBizMsgCrypt wxcpt = new WXBizMsgCrypt(token, encodingAESKey, corpId);
            int result = wxcpt.VerifyURL(signature, timestamp, nonce, echostr, ref retEchostr);
            if (result != 0)
            {
                return false;
            }

            return true;

            //ret==0表示验证成功，retEchostr参数表示明文，用户需要将retEchostr作为get请求的返回参数，返回给企业号。
            // HttpUtils.SetResponse(retEchostr);
        }
    }
}