                                          GIỚI THIỆU           

Tựa game này dựa trên cơ chế gameplay của Break the Sun (https://play.google.com/store/apps/details?id=com.bigdog.games.breakthesun&hl=vi)

Đồng thời mở rộng cơ chế với hai góc nhìn: First Person và Third Person.

Bên cạnh đó, game còn kết hợp yếu tố Idle, cho phép người chơi nâng cấp các thuộc tính nhằm tăng khả năng vượt qua các cấp độ một cách dễ dàng hơn.

Mục đích:

Chuyển từ phát triển game 2D sang 3D, làm quen với hệ trục OXYZ, hình học 3D, quản lý model, material, animation Humanoid, tối ưu hiệu suất và xử lý vật lý.

Trải nghiệm phát triển game mobile (Idle, Casual, Arcade), tối ưu hiệu suất, UI/UX cho màn hình cảm ứng.

Mở rộng kỹ năng thiết kế gameplay đơn giản nhưng cuốn hút, tập trung vào cơ chế Idle và tăng trưởng cho game di động.


SOLUTIION SHATTER OBJECT CẢI TIẾN HƠN SO VỚI BREAK THE SUN:


Phỏng đoán trong Brick The Sun: Các Object bị vỡ hoàn toàn khi được trigger, các Object vỡ 1 phần thật ra là các object con được xếp lên nhau, 
cách làm này dẫn tới 1 điểm yếu, khi ta muốn object vỡ 1 phần mỗi lần đập (vỡ theo số damage tác động lên Hp của gạch), ta cần ghép các object con có hình dạng giống nhau ở đỉnh và đáy, dẫn đến hạn chế số hình dạng của Object có thể đập được.

MY SOLUTION:

Yêu cầu: Dùng Rayfire for Unity

Các bước thực hiện:

Bước 1: Add component Rayfire Shatter, sau đó tinh chỉnh số mảnh sau khi shatter cũng như type của các mảnh,  để tạo 1 gameobject_root, loại gameobject đã được shatter thành nhiều mảnh (gameobject con)

![Image](https://github.com/user-attachments/assets/cc51e174-4957-46ac-be26-4957bde20206)

Bước 2:- Define 1 Gameobject DestructibleBrick gồm 3 game object con:

![Image](https://github.com/user-attachments/assets/28f4d45b-2c00-43fc-85ea-0b6d348eee06)

- gameobject_root:  

 + Component Rayfire Rigid 

 + Tùy chọn thêm ( rayfire bust + rayfire Debris) để thêm effect bust hay debris

  * Setting cụ thể của rigid đọc trong prefab của project

- Activator:

  + Empty gameobject + Rayfire Activator 

  + Positon ban đầu của Activator nằm trên gamobject_root

- Bomb:

  + Empty gameobject + Rayfire bomb 

  + Bán kính của Bomb đủ bao phủ gameobject_root

 *  Ý tưởng:

- Ghi lại position Y của activator khi di chuyển từ trên gameobject_root

+Khi ở trên đầu gameobject_root = topPosition

+Khi activator chạm đáy của gameobject_root = bottomPostion

+Ví trí chạm + offset = offsetPostion

- Số gạch cần phá vỡ sẽ bằng % máu gây ra 

+ Activator sẽ di chuyển theo Y position 1 đoạn bằng % máu * (topPostion - offsetPositon).

+ Sau khi activator di chuyển tới vị trí mới, số mảnh gạch va chạm với activator sẽ được active (chuyển thành dynamic type).

- Dùng bomb - cho nổ phần mảnh gạch dynamic type.

- Lí do không dùng (topPosition - bottomPosition ), vì trường hợp % máu của gạch quá nhỏ, mảnh gạch thì không nhỏ như vậy sẽ dẫn tới tất cả mảnh gạch được phá hủy hết dù HP của gạch vẫn còn.

 + Do vậy khi HP của gạch <=0, ta di chuyển gạch tới bottomPosition và cho nổ hết phần mảnh gạch còn lại
