namespace Do_an.Validation
{
    public static class ValidationMessages
    {
        public const string Required =
            "Vui lòng nhập {0}.";

        public const string StringLength =
            "{0} không được vượt quá {1} ký tự.";

        public const string Selection =
            "Vui lòng chọn {0} hợp lệ.";

        public const string Range =
            "{0} phải nằm trong khoảng từ {1} đến {2}.";

        public const string Email =
            "Địa chỉ email không hợp lệ.";

        public const string Compare =
            "{0} không khớp với {1}.";

        public const string DuplicateColorSize =
            "Màu và size này đã tồn tại trong sản phẩm.";

        public const string SaveFailed =
            "Chưa lưu được dữ liệu. Vui lòng thử lại.";
    }
}