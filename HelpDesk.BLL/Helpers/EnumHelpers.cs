using System.ComponentModel;

namespace HelpDesk.BLL.Helpers
{
    public static class EnumHelpers
    {
        public static string GetDescription<T>(T item)
        {
            object descriptionAttributes = typeof(T)
                .GetMember(typeof(T)
                .GetEnumName(item))[0]
                .GetCustomAttributes(typeof(DescriptionAttribute), false)[0];

            return ((DescriptionAttribute)descriptionAttributes).Description;
        }
    }
}
