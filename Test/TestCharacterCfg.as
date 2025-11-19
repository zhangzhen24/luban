

USTRUCT()
struct FTestCharacterCfg {
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Character", Meta = (DisplayName = "角色ID"))
	int32 ID = 0;

	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Character", Meta = (DisplayName = "角色名称"))
	FString Name = "";

	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Character", Meta = (DisplayName = "性别"))
	ECharacterGender Gender = ECharacterGender::None;

	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Character", Meta = (DisplayName = "种族"))
	ECharacterRace Race = ECharacterRace::None;
}
