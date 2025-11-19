UENUM()
enum ECharacterGender {
	None UMETA(DisplayName = "无"),
	Male UMETA(DisplayName = "男性"),
	Female UMETA(DisplayName = "女性"),
}


UENUM()
enum ECharacterRace {
	None UMETA(DisplayName = "无"),
	Human UMETA(DisplayName = "人类"),
	Duck UMETA(DisplayName = "鸭子"),
}



USTRUCT()
struct FCreateCharacterCfg {
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Character", Meta = (DisplayName = "角色ID"))
	int32 ID = 0;

	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Character", Meta = (DisplayName = "角色名称"))
	FString Name = "";

	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Character", Meta = (DisplayName = "性别"))
	ECharacterGender Gender = ECharacterGender::None;

	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Character", Meta = (DisplayName = "种族"))
	ECharacterRace Race = ECharacterRace::None;
}
