
const PRODUCTS = [
    {
        id: 1, name: 'Nike Air Zoom Pegasus 40', category: 'giay', brand: 'Nike',
        price: 3200000, oldPrice: 3800000, badge: 'sale',
        img: 'Images/1/1.jpg',
        imgs: ['Images/1/1.jpg', 'Images/1/2.jpg', 'Images/1/3.jpg'],
        sizes: ['38', '39', '40', '41', '42', '43', '44'], unavailable: ['43'],
        desc: 'Nike Air Zoom Pegasus 40 là người bạn đồng hành lý tưởng cho mọi cung đường chạy. Đệm Air Zoom mang lại cảm giác bật nảy mạnh mẽ từng bước chân, trong khi phần upper lưới thoáng khí giữ cho bàn chân luôn khô ráo. Thiết kế linh hoạt phù hợp cả tập luyện lẫn thi đấu.',
        specs: [['Chất liệu upper', 'Lưới kỹ thuật thoáng khí'], ['Đế ngoài', 'Cao su chịu mài mòn Nike'], ['Đệm', 'Air Zoom'], ['Trọng lượng', '280g (size 42)'], ['Xuất xứ', 'Vietnam'], ['Bảo hành', '12 tháng']]
    },
    {
        id: 2, name: 'Adidas Ultraboost 23', category: 'giay', brand: 'Adidas',
        price: 4500000, oldPrice: null, badge: 'new',
        img: 'Images/2/1.jpg',
        imgs: ['Images/2/1.jpg', 'Images/2/2.jpg', 'Images/2/3.jpg'],
        sizes: ['39', '40', '41', '42', '43', '44'], unavailable: ['44'],
        desc: 'Adidas Ultraboost 23 nâng tầm trải nghiệm chạy bộ với công nghệ BOOST™ thế hệ mới. Năng lượng hoàn trả tối đa, upper Primeknit ôm sát bàn chân như được đo riêng. Đây là sự kết hợp hoàn hảo giữa hiệu suất và phong cách.',
        specs: [['Chất liệu upper', 'Primeknit+'], ['Đế ngoài', 'Continental Rubber'], ['Đệm', 'BOOST™ thế hệ mới'], ['Trọng lượng', '310g (size 42)'], ['Xuất xứ', 'Indonesia'], ['Bảo hành', '12 tháng']]
    },
    {
        id: 3, name: 'Puma RS-X Efekt', category: 'giay', brand: 'Puma',
        price: 2100000, oldPrice: 2600000, badge: 'sale',
        img: 'Images/3/1.jpg',
        imgs: ['Images/3/1.jpg', 'Images/3/2.jpg'],
        sizes: ['38', '39', '40', '41', '42', '43'], unavailable: [],
        desc: 'Puma RS-X Efekt mang đến phong cách retro-futuristic ấn tượng với thiết kế bold, nhiều lớp vật liệu đa màu sắc. Đế RS (Running System) cổ điển được tái sinh với sức bật và êm ái vượt trội.',
        specs: [['Chất liệu upper', 'Da tổng hợp + Lưới'], ['Đế ngoài', 'Cao su tổng hợp'], ['Đệm', 'RS System'], ['Trọng lượng', '350g (size 42)'], ['Xuất xứ', 'China'], ['Bảo hành', '6 tháng']]
    },
    {
        id: 4, name: 'Nike Pro Compression Tee', category: 'ao', brand: 'Nike',
        price: 850000, oldPrice: null, badge: 'new',
        img: 'Images/4/1.jpg',
        imgs: ['Images/4/1.jpg', 'Images/4/2.jpg'],
        sizes: ['S', 'M', 'L', 'XL', 'XXL'], unavailable: [],
        desc: 'Nike Pro Compression Tee được thiết kế ôm sát cơ thể, hỗ trợ cơ bắp tối ưu trong từng động tác. Vải Dri-FIT ADV thế hệ mới thoát ẩm cực nhanh, giảm ma sát và giữ nhiệt độ cơ thể ổn định suốt buổi tập cường độ cao.',
        specs: [['Chất liệu', '85% Polyester, 15% Spandex Dri-FIT ADV'], ['Fit', 'Tight/Compression fit'], ['Cổ áo', 'Cổ tròn'], ['Tay áo', 'Ngắn'], ['Xuất xứ', 'Vietnam'], ['Bảo hành', '3 tháng']]
    },
    {
        id: 5, name: 'Adidas Techfit Legging', category: 'ao', brand: 'Adidas',
        price: 720000, oldPrice: 950000, badge: 'sale',
        img: 'Images/5/1.jpg',
        imgs: ['Images/5/1.jpg', 'Images/5/2.jpg'],
        sizes: ['XS', 'S', 'M', 'L', 'XL'], unavailable: ['XS'],
        desc: 'Adidas Techfit Legging với công nghệ TECHFIT compression hỗ trợ cơ đùi và bắp chân trong suốt buổi tập. Vải 4 chiều co giãn mạnh, đường may phẳng không cọ xát, lưng cao giữ dáng tôn vóc. Phù hợp chạy bộ, gym, yoga và mọi môn thể thao.',
        specs: [['Chất liệu', '78% Polyester, 22% Elastane TECHFIT'], ['Fit', 'Compression fit'], ['Lưng', 'Lưng cao có dây rút'], ['Túi', '1 túi nhỏ bên hông'], ['Xuất xứ', 'Cambodia'], ['Bảo hành', '3 tháng']]
    },
    {
        id: 6, name: 'Under Armour Rival Fleece Hoodie', category: 'ao', brand: 'Under Armour',
        price: 1250000, oldPrice: null, badge: null,
        img: 'Images/6/1.jpg',
        imgs: ['Images/6/1.jpg', 'Images/6/2.jpg'],
        sizes: ['S', 'M', 'L', 'XL', 'XXL'], unavailable: ['S'],
        desc: 'Under Armour Rival Fleece Hoodie với lớp lót fleece dày dặn giữ ấm tuyệt vời trong thời tiết lạnh. Thiết kế oversize vừa phong cách vừa thoải mái. Túi kangaroo phía trước rộng rãi, mũ có dây điều chỉnh. Lý tưởng cho khởi động, warm-down hoặc mặc hàng ngày.',
        specs: [['Chất liệu', '80% Cotton, 20% Polyester Rival Fleece'], ['Fit', 'Oversize fit'], ['Mũ', 'Có mũ điều chỉnh dây'], ['Túi', 'Túi kangaroo phía trước'], ['Xuất xứ', 'Jordan'], ['Bảo hành', '3 tháng']]
    },
    {
        id: 7, name: 'Tạ Đôi Điều Chỉnh 20kg', category: 'gym', brand: 'Puma',
        price: 1800000, oldPrice: null, badge: 'new',
        img: 'Images/7/1.jpg',
        imgs: ['Images/7/1.jpg'],
        sizes: ['5kg', '10kg', '15kg', '20kg'], unavailable: [],
        desc: 'Bộ tạ đôi điều chỉnh tiện lợi, tiết kiệm không gian. Có thể thay đổi trọng lượng từ 2kg đến 20kg chỉ trong vài giây. Bề mặt tay cầm được phủ lớp cao su chống trơn trượt an toàn.',
        specs: [['Vật liệu', 'Gang đúc + Cao su'], ['Trọng lượng tối đa', '20kg/tạ'], ['Đường kính tay cầm', '32mm'], ['Chiều dài', '42cm'], ['Xuất xứ', 'China'], ['Bảo hành', '24 tháng']]
    },
    {
        id: 8, name: 'Dây Kháng Lực Bộ 5 Dây', category: 'gym', brand: 'Under Armour',
        price: 320000, oldPrice: 420000, badge: 'sale',
        img: 'Images/8/1.jpg',
        imgs: ['Images/8/1.jpg', 'Images/8/2.jpg'],
        sizes: ['Bộ 5 dây'], unavailable: [],
        desc: 'Bộ 5 dây kháng lực với các mức lực kéo khác nhau từ 5kg đến 40kg. Chất liệu cao su tự nhiên bền bỉ, không gãy. Kèm theo tay cầm bọc mút, móc cửa và đai buộc cổ chân, phù hợp đủ loại bài tập.',
        specs: [['Vật liệu', 'Cao su tự nhiên'], ['Số dây', '5 dây'], ['Lực kéo', '5 / 10 / 15 / 25 / 40kg'], ['Chiều dài', '120cm mỗi dây'], ['Xuất xứ', 'China'], ['Bảo hành', '12 tháng']]
    },
    {
        id: 9, name: 'Thảm Tập Yoga 6mm', category: 'gym', brand: 'Adidas',
        price: 480000, oldPrice: null, badge: null,
        img: 'Images/9/1.jpg',
        imgs: ['Images/9/1.jpg', 'Images/9/2.jpg'],
        sizes: ['61x173cm', '61x183cm'], unavailable: [],
        desc: 'Thảm yoga Adidas dày 6mm với bề mặt chống trơn trượt hai mặt, đảm bảo an toàn trong mọi tư thế. Chất liệu TPE thân thiện môi trường, không mùi, dễ vệ sinh. Nhẹ và có dây đai cuộn tiện lợi.',
        specs: [['Vật liệu', 'TPE cao cấp'], ['Độ dày', '6mm'], ['Kích thước', '61 x 173cm'], ['Trọng lượng', '0.9kg'], ['Xuất xứ', 'China'], ['Bảo hành', '12 tháng']]
    },
    {
        id: 10, name: 'Bóng Đá Nike Premier League', category: 'bongda', brand: 'Nike',
        price: 950000, oldPrice: 1100000, badge: 'sale',
        img: 'Images/10/1.jpg',
        imgs: ['Images/10/1.jpg'],
        sizes: ['Size 3', 'Size 4', 'Size 5'], unavailable: [],
        desc: 'Bóng đá Nike chính hãng với thiết kế sắc nét, đường khâu chính xác và độ nảy ổn định trên mọi mặt sân. Được sử dụng trong các giải đấu phong trào và thi đấu chuyên nghiệp.',
        specs: [['Tiêu chuẩn', 'FIFA Quality Pro'], ['Chất liệu', 'Polyurethane cao cấp'], ['Ruột bóng', 'Butyl 100%'], ['Số lớp lót', '4 lớp'], ['Xuất xứ', 'Pakistan'], ['Bảo hành', '6 tháng']]
    },
    {
        id: 11, name: 'Găng Thủ Môn Adidas Predator', category: 'bongda', brand: 'Adidas',
        price: 780000, oldPrice: null, badge: 'new',
        img: 'Images/11/1.jpg',
        imgs: ['Images/11/1.jpg', 'Images/11/2.jpg'],
        sizes: ['Size 6', 'Size 7', 'Size 8', 'Size 9', 'Size 10'], unavailable: [],
        desc: 'Găng tay thủ môn Adidas Predator với lớp mút Supersoft latex cho khả năng bắt bóng vượt trội. Thiết kế negative cut ôm sát từng ngón tay, tăng cảm giác bóng và kiểm soát tốt nhất.',
        specs: [['Lớp mút', 'Supersoft Latex 4mm'], ['Kiểu cắt', 'Negative Cut'], ['Cổ tay', 'Dán Velcro điều chỉnh'], ['Ngón trỏ', 'Thanh chống gãy'], ['Xuất xứ', 'Pakistan'], ['Bảo hành', '6 tháng']]
    },
    {
        id: 12, name: 'Bình Nước Thể Thao 1L', category: 'phukien', brand: 'Nike',
        price: 280000, oldPrice: 350000, badge: 'sale',
        img: 'Images/12/1.jpg',
        imgs: ['Images/12/1.jpg', 'Images/12/2.jpg'],
        sizes: ['500ml', '750ml', '1000ml'], unavailable: [],
        desc: 'Bình nước thể thao Nike dung tích 1 lít với nắp bật lò xo tiện lợi, uống nhanh không cần mở nắp. Nhựa Tritan không chứa BPA, an toàn cho sức khỏe. Thiết kế ergonomic dễ cầm nắm khi vận động.',
        specs: [['Vật liệu', 'Nhựa Tritan không BPA'], ['Dung tích', '1000ml'], ['Nắp', 'Bật lò xo có khóa'], ['Rửa', 'Được rửa máy rửa bát'], ['Xuất xứ', 'China'], ['Bảo hành', '12 tháng']]
    },
    {
        id: 13, name: 'Băng Cổ Tay Nike', category: 'phukien', brand: 'Nike',
        price: 150000, oldPrice: null, badge: null,
        img: 'Images/13/1.jpg',
        imgs: ['Images/13/1.jpg', 'Images/13/2.jpg'],
        sizes: ['S/M', 'L/XL'], unavailable: [],
        desc: 'Băng cổ tay Nike thấm hút mồ hôi hiệu quả, giữ cho tay luôn khô trong lúc thi đấu hoặc tập luyện. Chất liệu terry cotton mềm mại, co giãn tốt. Bộ 2 chiếc tiện dụng cho cả hai tay.',
        specs: [['Chất liệu', '80% Cotton, 17% Polyester, 3% Elastane'], ['Kích thước', '7 x 10cm'], ['Số lượng', '2 chiếc / bộ'], ['Màu sắc', 'Trắng / Đen / Đỏ'], ['Xuất xứ', 'Vietnam'], ['Bảo hành', '3 tháng']]
    },
    {
        id: 14, name: 'Ba Lô Adidas Classic', category: 'phukien', brand: 'Adidas',
        price: 1200000, oldPrice: 1500000, badge: 'sale',
        img: 'Images/14/1.jpg',
        imgs: ['Images/14/1.jpg', 'Images/14/2.jpg'],
        sizes: ['One Size'], unavailable: [],
        desc: 'Ba lô Adidas Classic 3 Stripes với thiết kế tối giản nhưng đẳng cấp. Ngăn chính rộng rãi chứa vừa laptop 15 inch, ngăn phụ tiện lợi cho đồ dùng nhỏ. Dây đeo có đệm mút êm vai, phù hợp mang cả ngày.',
        specs: [['Chất liệu', 'Polyester 100% tái chế'], ['Dung tích', '21 lít'], ['Ngăn laptop', 'Lên đến 15 inch'], ['Kích thước', '30 x 47 x 14cm'], ['Xuất xứ', 'Vietnam'], ['Bảo hành', '12 tháng']]
    },
    {
        id: 15, name: 'Under Armour HOVR Phantom 3', category: 'giay', brand: 'Under Armour',
        price: 3800000, oldPrice: null, badge: 'new',
        img: 'Images/15/1.jpg',
        imgs: ['Images/15/1.jpg', 'Images/15/2.jpg', 'Images/15/3.jpg'],
        sizes: ['38', '39', '40', '41', '42', '43', '44'], unavailable: ['38'],
        desc: 'Under Armour HOVR Phantom 3 tích hợp công nghệ HOVR™ giúp "triệt tiêu" lực tác động và hoàn trả năng lượng tối đa. Cảm biến kết nối với app MapMyRun tự động theo dõi cadence và chỉnh kỹ thuật chạy.',
        specs: [['Chất liệu upper', 'UA Warp Knit thoáng khí'], ['Đế ngoài', 'Cao su ngoài trơn'], ['Đệm', 'HOVR™ + EVA'], ['Trọng lượng', '295g (size 42)'], ['Xuất xứ', 'Vietnam'], ['Bảo hành', '12 tháng']]
    },
    {
        id: 16, name: 'Puma Evostripe Jogger Pants', category: 'ao', brand: 'Puma',
        price: 650000, oldPrice: 820000, badge: 'sale',
        img: 'Images/16/1.jpg',
        imgs: ['Images/16/1.jpg', 'Images/16/2.jpg'],
        sizes: ['S', 'M', 'L', 'XL', 'XXL'], unavailable: [],
        desc: 'Puma Evostripe Jogger Pants kết hợp vải fleece nhẹ giữ ấm với thiết kế jogger năng động. Gấu quần bo chun gọn gàng, cạp có dây rút điều chỉnh. Hai túi trước và một túi sau đủ đựng điện thoại và đồ dùng cá nhân. Phù hợp tập gym, chạy bộ nhẹ và mặc hàng ngày.',
        specs: [['Chất liệu', '68% Cotton, 32% Polyester Fleece'], ['Fit', 'Tapered fit'], ['Cạp', 'Dây rút điều chỉnh'], ['Túi', '2 túi trước + 1 túi sau'], ['Xuất xứ', 'Bangladesh'], ['Bảo hành', '3 tháng']]
    },
    {
        id: 17, name: 'Nike Air Max 270', category: 'giay', brand: 'Nike',
        price: 4200000, oldPrice: null, badge: 'new',
        img: 'Images/17/1.jpg',
        imgs: ['Images/17/1.jpg', 'Images/17/2.jpg', 'Images/17/3.jpg'],
        sizes: ['38', '39', '40', '41', '42', '43', '44'], unavailable: [],
        desc: 'Nike Air Max 270 sở hữu đơn vị đệm Air lớn nhất từ trước đến nay ở phần gót, mang lại cảm giác êm ái suốt cả ngày dài. Thiết kế lifestyle năng động với upper lưới kỹ thuật nhẹ và đường nét bo tròn hiện đại, phù hợp cả đường phố lẫn phòng tập.',
        specs: [['Chất liệu upper', 'Lưới kỹ thuật + da tổng hợp'], ['Đế ngoài', 'Cao su chịu mài mòn'], ['Đệm', 'Air Max 270 (đệm gót 270°)'], ['Trọng lượng', '298g (size 42)'], ['Xuất xứ', 'Vietnam'], ['Bảo hành', '12 tháng']]
    },
    {
        id: 18, name: 'Adidas Tiro 23 Training Jacket', category: 'ao', brand: 'Adidas',
        price: 980000, oldPrice: null, badge: 'new',
        img: 'Images/18/1.jpg',
        imgs: ['Images/18/1.jpg', 'Images/18/2.jpg'],
        sizes: ['S', 'M', 'L', 'XL', 'XXL'], unavailable: [],
        desc: 'Adidas Tiro 23 Training Jacket là chiếc áo khoác tập luyện biểu tượng được hàng triệu cầu thủ tin dùng. Vải AEROREADY thấm hút mồ hôi tức thì, co giãn 4 chiều theo mọi chuyển động. Cổ đứng và đường sọc 3 vạch cổ điển tạo nên phong cách đặc trưng không nhầm lẫn.',
        specs: [['Chất liệu', '100% Polyester tái chế AEROREADY'], ['Fit', 'Regular fit'], ['Cổ', 'Cổ đứng có khoá kéo'], ['Túi', '2 túi bên có khoá kéo'], ['Xuất xứ', 'Cambodia'], ['Bảo hành', '3 tháng']]
    },
    {
        id: 19, name: 'Puma King Pro FG', category: 'giay', brand: 'Puma',
        price: 2800000, oldPrice: null, badge: 'new',
        img: 'Images/19/1.jpg',
        imgs: ['Images/19/1.jpg', 'Images/19/2.jpg'],
        sizes: ['38', '39', '40', '41', '42', '43', '44'], unavailable: ['44'],
        desc: 'Puma King Pro FG hồi sinh huyền thoại với lớp da kangaroo cao cấp mềm mại cho cảm giác bóng tuyệt hảo. Đế ngoài FG tối ưu lực bám trên sân cỏ tự nhiên, đinh conical giúp xoay người linh hoạt. Biểu tượng một thời nay trở lại với công nghệ hiện đại.',
        specs: [['Chất liệu upper', 'Da Kangaroo tự nhiên'], ['Đế ngoài', 'FG (Firm Ground)'], ['Đinh', 'Conical + Blade kết hợp'], ['Trọng lượng', '240g (size 42)'], ['Xuất xứ', 'Vietnam'], ['Bảo hành', '6 tháng']]
    },
];

// ==================== ĐÁNH GIÁ ====================
// Dùng chung, hiển thị ở trang chi tiết sản phẩm
const DANH_GIA = [
    { user: 'Nguyễn Văn A', rating: 5, date: '12/04/2025', text: 'Sản phẩm rất tốt, đúng như mô tả. Giao hàng nhanh, đóng gói cẩn thận. Sẽ ủng hộ lần sau!' },
    { user: 'Trần Thị B', rating: 4, date: '05/04/2025', text: 'Chất lượng ổn, mang khá êm. Trừ 1 sao vì size hơi lớn hơn so với thông thường, nên order nhỏ hơn 1 size.' },
    { user: 'Lê Minh C', rating: 5, date: '28/03/2025', text: 'Hàng chính hãng, màu sắc đẹp như ảnh. Rất hài lòng!' },
];