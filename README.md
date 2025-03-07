Giới thiệu:

Tựa game này được phát triển dựa trên cơ chế gameplay của Break the Sun, đồng thời mở rộng với hai góc nhìn: First Person và Third Person.

Bên cạnh đó, game còn kết hợp yếu tố Idle, cho phép người chơi nâng cấp các thuộc tính nhằm tăng khả năng vượt qua các cấp độ một cách dễ dàng hơn.

![Image](https://github.com/user-attachments/assets/17668eba-d20f-46a8-abb5-e988aeb6e56f)

Mục đích:

Thử sức với phát triển game 3D, vì trước đây chủ yếu làm game 2D. 
Điều này giúp làm quen với việc làm việc trên trục OXYZ, hình học 3D, 
quản lý model, material, các loại type của model, tái sử dụng animation với Humanoid, 
tối ưu hóa hiệu suất và dung lượng cho game 3D, cũng như xử lý các vấn đề liên quan đến vật lý trong môi trường 3D.

Trải nghiệm phát triển game mobile thuộc thể loại Idle, Casual, Arcade, do trước đây chủ yếu làm game trên PC. 
Điều này giúp làm quen với tối ưu hóa hiệu suất cho thiết bị di động, quản lý UI/UX cho màn hình cảm ứng,

Mở rộng kỹ năng thiết kế gameplay đơn giản nhưng gây nghiện, 
tập trung vào cơ chế Idle để giữ chân người chơi, đồng thời thử nghiệm các cách tăng trưởng và monetization phù hợp với game di động.


Solution Giải quyết đập gạch tối ưu hơn so với game gốc Break The Sun
-Cách để làm 1 loại gạch mới trở thành một loại gạch có thể bị đập được

Add component Rayfire Shatter có game object - để tạo 1 gameobject_root, loại gameobject đã được shatter thành nhiều mảnh

- Define 1 Gameobject DestructibleBrick gồm 3 game object con:
+ gameobject_root: (+ component Rayfire Rigid) - tùy chọn thêm ( rayfire bust + rayfire Debris)
  * Setting của rigid đọc trong project
+ Activator (empty gameobject + Rayfire Activator) - vị trí ban đầu nằm trên gamobject_root
+ Bomb (empty gameobject + Rayfire bomb) - bán kính đủ bao phủ gameobject_root
- Ghi lại vị trí của activator khi di chuyển từ trên gameobject_root
+Khi ở trên đầu gameobject_root = topPosition
+Khi activator chạm đáy của gameobject_root = bottomPostion
+Ví trí chạm + offset = offsetPostion

- Số gạch cần phá vỡ sẽ bằng % máu gây ra
+ activator sẽ di chuyển theo Y position 1 đoạn bằng % máu * (topPostion - offsetPositon) - công thức  xem trong code 
+ Sau khi activator di chuyển tới vị trí mới, số mảnh gạch va chạm với activator sẽ được active(dynamic type)
+ Dùng bomb - cho nổ phần mảnh gạch dynamic type
+ Lí do không dùng (topPosition - bottomPosition ), vì trường hợp % máu của gạch quá nhỏ, mảnh gạch thì không nhỏ như vậy sẽ dẫn tới tất cả mảnh gạch được phá hủy hết dù HP của gạch vẫn còn
+ Do vậy khi HP của gạch <=0, ta di chuyển gạch tới topbottom và cho nổ hết phần mảnh gạch còn lại
