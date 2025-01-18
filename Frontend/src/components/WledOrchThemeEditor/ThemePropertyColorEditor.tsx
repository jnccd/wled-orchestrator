import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  getWledOrchState,
  setGroupTheme,
  wledOrchStateQueryKey,
} from "../../hooks/useWledOrchState";
import useSelectedGroupStore from "../../hooks/useLocalStore";
import { Colorful, ColorResult, rgbaToHsva } from "@uiw/react-color";
import { useState } from "react";
import { writeProperty } from "../../utils/untypedPropertyAccess";

interface Props {
  propertyName: string;
  propertyValue: any;
}

const ThemePropertyColorEditor = ({ propertyName, propertyValue }: Props) => {
  const [refreshBool, refresh] = useState(false);

  // React Query setup
  const queryClient = useQueryClient();
  const wledOrchStateQuery = useQuery({
    queryKey: [wledOrchStateQueryKey],
    queryFn: getWledOrchState,
  });
  const changeThemeMutation = useMutation({
    mutationFn: setGroupTheme,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [wledOrchStateQueryKey] });
    },
  });

  const selectedGroupStore = useSelectedGroupStore();
  const selectedGroup = wledOrchStateQuery.data?.groups?.filter(
    (x) => x.id == selectedGroupStore.selectedGroup
  )[0];

  return (
    <Colorful
      color={rgbaToHsva({
        r: propertyValue.r,
        g: propertyValue.g,
        b: propertyValue.b,
        a: 1,
      })}
      onChange={(colorRes: ColorResult) => {
        writeProperty(selectedGroup?.theme, propertyName, colorRes.rgb);
        refresh(!refreshBool);
      }}
      onMouseUp={() => {
        changeThemeMutation.mutate({
          groupId: selectedGroup?.id ?? "",
          newTheme: selectedGroup?.theme,
        });
      }}
      disableAlpha
    ></Colorful>
  );
};

export default ThemePropertyColorEditor;
