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
