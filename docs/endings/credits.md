# 梦结局制作人员名单

对话结束后继续躺卧36秒，前16秒保留天空，后20秒叠加居中电影式上滚名单，随后执行原有1秒淡黑并返回菜单。名单使用工程中的Ark像素字形与黄色（#FFCF68），只显示中文姓名；背景仍是躺卧看天画面。暂停冻结计时，读档按结局阶段时间恢复滚动位置。[verified: 用户2026-10-26请求；UnityProject/Assets/MiniGame/Runtime/DreamEndingController.cs RefreshCredits；DreamEndingCredits.cs]

| 职能 | 人员 |
| --- | --- |
| 策划 | 马步庭、时晨洋 |
| 美术 | 刘静怡、刘皓喆、赵耀 |
| 程序 | 李海江、黄炜轩 |
| 发行 | 胡雅淳、吴芷娴、黎念祺 |
| 运营 | 陈瑶瑶、朱睿雯、时子彧、黄郑志一 |

名单依据用户提供的截图录入，两行运营合并成同一职能组。[verified: 用户2026-10-26最终文字名单]

本次按用户要求先提交并推送工程改动，再生成Windows新包；此前10秒版本压缩包属于旧版本。[verified: 用户2026-10-26最新请求]

两项PlayMode测试通过，覆盖36秒边界、16秒后出现、20秒上滚、中文名单与字形、暂停冻结及存读档恢复。名单版式沿用已检查的初段、中段和末段1920×1080叠加画面，本轮更新停留与滚动时长，验证记录见[credits-verification.json](credits-verification.json)。这些画面由真实房间相机与名单画布分层渲染，没有手动替换名单或字形；本轮构建结果另随新包保存。[verified: Artifacts/DreamEndingCredits36s/timing-test.json；Artifacts/DreamEndingCredits36s/checkpoint-test.json；Artifacts/DreamEndingCredits/VerifiedCaptures]
