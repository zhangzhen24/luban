UENUM()
enum EItemType {
	Default UMETA(DisplayName = "默认"),
	Currency UMETA(DisplayName = "货币"),
	Consumable UMETA(DisplayName = "消耗品"),
	Equipment UMETA(DisplayName = "装备"),
	Material UMETA(DisplayName = "材料"),
	Quest UMETA(DisplayName = "任务道具"),
	Key UMETA(DisplayName = "钥匙卡"),
	Summon UMETA(DisplayName = "召唤物"),
	Test UMETA(DisplayName = "测试道具"),
}


UENUM()
enum EItemRarity {
	White UMETA(DisplayName = "白"),
	Green UMETA(DisplayName = "绿"),
	Blue UMETA(DisplayName = "蓝"),
	Purple UMETA(DisplayName = "紫"),
	Orange UMETA(DisplayName = "橙"),
	Red UMETA(DisplayName = "红"),
}


UENUM()
enum EItemConditionType {
	CheckLevel UMETA(DisplayName = "检查等级"),
	MatchTag UMETA(DisplayName = "匹配Tag"),
	MatchTags UMETA(DisplayName = "匹配Tags"),
}



USTRUCT()
struct FItemRarityColorCfg {
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "ItemRarityColor", Meta = (DisplayName = "道具稀有度颜色ID"))
	int32 ID = 0;

	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "ItemRarityColor", Meta = (DisplayName = "物品稀有度"))
	EItemRarity ItemRarity = EItemRarity::White;

	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "ItemRarityColor", Meta = (DisplayName = "物品稀有度颜色"))
	FLinearColor Color = FLinearColor::White;
}


USTRUCT()
struct FItemCfg {
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Item", Meta = (DisplayName = "道具ID"))
	int32 ID = 0;

	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Item", Meta = (DisplayName = "道具名称"))
	FString DisplayName = "";

	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Item", Meta = (DisplayName = "道具描述"))
	FString Description = "";

	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Item", Meta = (DisplayName = "道具类型"))
	EItemType ItemType = EItemType::Default;

	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Item", Meta = (DisplayName = "道具稀有度"))
	EItemRarity ItemRarity = EItemRarity::White;
}
