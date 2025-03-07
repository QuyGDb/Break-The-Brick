                                                GIỚI THIỆU           

🕹 Tựa game này dựa trên cơ chế gameplay của Break the Sun

🔗 Break the Sun trên Google Play: https://play.google.com/store/apps/details?id=com.bigdog.games.breakthesun&hl=vi

📌 Mở rộng gameplay với:

👁 Hai góc nhìn: First Person & Third Person

⏳ Kết hợp yếu tố Idle: Nâng cấp thuộc tính giúp vượt qua các cấp độ dễ dàng hơn

🎯 Mục đích

🔹 Chuyển từ phát triển game 2D sang 3D

Làm quen với hệ trục OXYZ, hình học 3D, quản lý model, material, animation Humanoid
Tối ưu hiệu suất & xử lý vật lý trong môi trường 3D

🔹 Trải nghiệm phát triển game mobile

Làm game thể loại Idle, Casual, Arcade
Tối ưu hiệu suất, UI/UX cho màn hình cảm ứng

🔹 Mở rộng kỹ năng thiết kế gameplay

Gameplay đơn giản nhưng cuốn hút
Tập trung vào cơ chế Idle & tăng trưởng trong game di động


🔨 SOLUTION SHATTER OBJECT – CẢI TIẾN SO VỚI BREAK THE SUN

⚙ Phỏng đoán cách hoạt động trong Break the Sun:

📌 Các object bị vỡ hoàn toàn khi bị trigger

📌 Các object vỡ một phần thực chất là các object con xếp chồng lên nhau

🚨 Hạn chế:

Khi muốn object vỡ từng phần dựa theo damage, cần ghép các object con có hình dạng giống nhau ở đỉnh và đáy

Điều này hạn chế số hình dạng của object có thể bị đập


💡 MY SOLUTION: 

Giúp đập vỡ từng phần của object mà không cần xếp các object lại với nhau, giúp object có thể nhiều hình dạng các nhau

✅ Yêu cầu:

✔ Dùng Rayfire for Unity

⚙ Các bước thực hiện:

🔹 Bước 1: Thêm component Rayfire Shatter

Tinh chỉnh số mảnh shatter & loại mảnh

Tạo gameobject_root, chứa các mảnh đã bị shatter

📷 Hình minh họa:

![Image](https://github.com/user-attachments/assets/cc51e174-4957-46ac-be26-4957bde20206)

🔹 Bước 2: Define GameObject DestructibleBrick, gồm 3 thành phần:

📷 Hình minh họa:

![Image](https://github.com/user-attachments/assets/28f4d45b-2c00-43fc-85ea-0b6d348eee06)

1️⃣ gameobject_root:

🛠 Component Rayfire Rigid

✨ Tùy chọn Rayfire Bust + Rayfire Debris để thêm hiệu ứng nổ, mảnh vỡ

📌 Cài đặt cụ thể xem trong prefab của project

2️⃣ Activator:

🔹 Empty GameObject + Rayfire Activator

🔹 Vị trí ban đầu nằm trên gameobject_root

3️⃣ Bomb:

💥 Empty GameObject + Rayfire Bomb

📏 Bán kính Bomb đủ bao phủ toàn bộ gameobject_root

💡 Ý TƯỞNG HOẠT ĐỘNG

📌 Ghi lại vị trí Y của Activator khi di chuyển trên gameobject_root

🟢 Khi ở trên đầu: topPosition

🔴 Khi chạm đáy: bottomPosition

📍 Vị trí chạm + offset: offsetPosition

📌 Tính số gạch cần phá vỡ dựa vào % máu mất đi

Activator di chuyển theo trục Y một đoạn bằng:

🏗 % máu * (topPosition - offsetPosition)

Khi Activator di chuyển tới vị trí mới, các mảnh gạch chạm Activator sẽ được active (dynamic type)

Sau đó, Bomb nổ phá hủy các mảnh gạch dynamic type

🚨 Lý do không dùng (topPosition - bottomPosition):

Nếu % máu quá nhỏ, mà mảnh gạch lớn, sẽ khiến toàn bộ object bị phá hủy dù HP vẫn còn

🛠 Giải pháp: Khi HP gạch = 0, di chuyển Activator xuống bottomPosition và nổ toàn bộ gạch còn lại

📌 Unity Version: 2021.3.43f1

