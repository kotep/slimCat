#region Copyright

// <copyright file="NotEmptyBoolConverter.cs">
//     Copyright (c) 2013-2015, Justin Kadrovach, All rights reserved.
// 
//     This source is subject to the Simplified BSD License.
//     Please see the License.txt file for more information.
//     All other rights reserved.
// 
//     THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//     KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//     IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
//     PARTICULAR PURPOSE.
// </copyright>

#endregion

namespace slimCat.Utilities
{
    /// <summary>
    ///     If string is not empty, return true.
    /// </summary>
    public class NotEmptyBoolConverter : OneWayConverter
    {
        public override object Convert(object value, object parameter)
        {
            var parsed = (string) value;

            return !string.IsNullOrEmpty(parsed);
        }
    }
}