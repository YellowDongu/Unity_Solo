using UnityEngine;
using UnityEditor;
using System.IO;

public class EditorTools
{

    [MenuItem("Assets/Convert Selected DDS to PNG", false, 1)]
    public static void ConvertMultipleDDSToPNG()
    {
        // 1. 현재 선택된 모든 오브젝트를 가져옴
        Object[] selectedObjects = Selection.objects;
        if (selectedObjects == null || selectedObjects.Length == 0) return;

        int convertedCount = 0;

        foreach (Object obj in selectedObjects)
        {
            // Texture2D인지 확인 (DDS도 유니티 내에선 Texture2D로 인식됨)
            Texture2D ddsTex = obj as Texture2D;
            if (ddsTex == null) continue;

            string path = AssetDatabase.GetAssetPath(ddsTex);
            if (!path.ToLower().EndsWith(".dds")) continue;

            // 2. 개별 변환 로직 실행
            ProcessConversion(ddsTex, ref path);// 2. 중요: 유니티가 새 파일을 인식하도록 강제 로드
            AssetDatabase.ImportAsset(path);

            // 3. 코드로 임포트 세팅 변경 (TextureImporter 접근)
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite; // 스프라이트로 변경
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true; // 알파 채널 유지
                importer.mipmapEnabled = false; // UI용이면 밉맵 끄기 (성능 이득)

                // 설정 적용 및 재임포트
                importer.SaveAndReimport();
            }
            convertedCount++;
        }

        // 3. 에셋 데이터베이스 한 번만 갱신 (성능 최적화)
        AssetDatabase.Refresh();
        Debug.Log($"총 {convertedCount}개의 DDS 파일이 PNG로 변환되었습니다.");
    }

    private static void ProcessConversion(Texture2D source, ref string assetPath)
    {
        // 읽기 불가능한 DDS 데이터를 읽기 위해 RenderTexture 사용 (DX11의 Staging Buffer 개념)
        RenderTexture rt = RenderTexture.GetTemporary(source.width, source.height);
        Graphics.Blit(source, rt);
        RenderTexture.active = rt;

        Texture2D newTex = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        newTex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        newTex.Apply();

        byte[] bytes = newTex.EncodeToPNG();
        assetPath = assetPath.Replace(".dds", ".png");

        File.WriteAllBytes(assetPath, bytes);

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        // 메모리 해제
        Object.DestroyImmediate(newTex);
    }
}
