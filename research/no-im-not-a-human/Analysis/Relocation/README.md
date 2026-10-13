# 研究目录迁移记录

新研究根目录为仓库内 `research/no-im-not-a-human/`；该目录整份由 Git 忽略。[verified: relocation.json current_research_root；acceptance.json ignored_paths/tracked_research_paths]

源目录23,961个文件、1,218个目录、7,004,476,419字节已复制，复制前后SHA256一致。路径修正后再次完整校验，仅14个已记录文件发生预期变更。[verified: copy-verification.json files_checked/directories_checked/original_bytes/passed；final-integrity-verification.json changed_destination_files/passed]

- [迁移状态与历史路径映射](relocation.json)。[verified: relocation.json historical_path_policy]
- [原始文件清单](source-manifest.json)与[复制校验](copy-verification.json)。[verified: 相应JSON files/passed]
- [预期修改及前后哈希](changed-files.json)与[最终完整性校验](final-integrity-verification.json)。[verified: 相应JSON changes/passed]
- [验收状态](acceptance.json)与[独立复核](independent-reviews.json)。[verified: 相应JSON target_acceptance_passed/documentation/migration_code]
- [执行记录与例外](implementation-notes.md)。[verified: implementation-notes.md Deviations/Completion summary]

历史JSON和日志中的原D盘路径保留；仅将原研究根前缀映射到当前根，Steam源路径不变。活动脚本通过自身位置定位根目录。[verified: relocation.json historical_path_policy；Analysis/investigation-status.json source_game]

目标资料已验收；自动执行策略拒绝递归删除原研究目录，旧D盘副本保留，整项移动尚未完成源清理。未提交或推送Git变更。[verified: acceptance.json complete_move/source_removal；relocation.json git_scope]

本轮没有执行游戏、Unity导入或核心测试；文件迁移验证不构成可玩版本证明。[verified: acceptance.json core_tests_this_turn/unity_import_or_runtime_this_turn]
