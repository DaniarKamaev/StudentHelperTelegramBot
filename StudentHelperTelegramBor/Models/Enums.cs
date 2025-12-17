using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentHelperTelegramBot.Models
{
    public enum UserState
    {
        None,
        WaitingForEmail,
        WaitingForPassword,
        WaitingForRegEmail,
        WaitingForRegPassword,
        WaitingForRegFirstName,
        WaitingForRegLastName,
        WaitingForRegGroupId,
        WaitingForPublicationTitle,
        WaitingForPublicationContent,
        WaitingForPublicationType,
        WaitingForLectureSearchSubject,
        WaitingForGroupName,
        WaitingForAICategory,
        WaitingForAIQuestion,
        LoggingOut,
        WaitingForLectureTitle,      
        WaitingForLectureDescription,  
        WaitingForLectureAddSubject,
        WaitingForLectureExternalUrl
    }
}
