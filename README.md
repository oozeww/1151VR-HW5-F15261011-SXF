Unity作業五

尚烜妃-F15261011

專案截圖：

<img width="865" height="530" alt="image" src="https://github.com/user-attachments/assets/2cf04d4a-a3c0-43f6-b0c0-0f19e8e6a325" />

 
Github鏈接：

  https://github.com/oozeww/1151VR-HW5-F15261011-SXF

Youtube鏈接：

https://youtu.be/7xcHmTJquQY

製作流程：

1.	我新增了terrain空白地面，導入了三個模型：人物、寶可夢球、寶可夢

2.	場景中使用兩個空物體作為位置標記：ballStarPoint 放在訓練家旁邊，表示精靈球的起始位置。 CapturePoint 放在寶可夢附近，表示精靈球的目標位置，空物體用來提供坐標，腳本引用空物體來讀取坐標

3.	按下space鍵開始馴服寶可夢，脚本記錄起點、目標點、飛行時間，用vector3.Lerp根據進度在起點和目標點閒插值，把計算的坐標付給精靈球

4.	增加細節，添加馴服寶可夢的音效和特效
