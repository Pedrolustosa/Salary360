import { IconName } from '../../shared/icon-paths';

export interface NavItem {
  labelKey: string;
  route: string;
  icon: IconName;
  exact: boolean;
  permission?: string;
}
