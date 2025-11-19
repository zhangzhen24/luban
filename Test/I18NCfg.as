

USTRUCT()
struct FI18NCfg {
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "I18N", Meta = (DisplayName = "ID"))
	int32 ID = 0;

	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "I18N", Meta = (DisplayName = "英文文本"))
	FString English = "";

	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "I18N", Meta = (DisplayName = "中文文本"))
	FString Chinese = "";
}
