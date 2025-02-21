import { defineConfig } from 'vitepress'

const langName = '/zh';

export const zh = defineConfig({
  lang: 'zh-CN',
  description: '一个魔法物品栏系统，只需添加组件就能轻松创建菜单。',

  themeConfig: {
    logo: '/images/logo.svg',
    nav: [
      { text: '主页', link: langName + '/' },
      { text: '教程', link: langName + '/tutorial/', activeMatch: '/tutorial/' },
      { text: '文档', link: langName + '/docs/', activeMatch: '/docs/' },
      { text: 'API', link: langName + '/api/', activeMatch: '/api/' }
    ],

    sidebar: [
      {
        text: '教程',
        link: langName + '/tutorial/',
        collapsed: false,
        items: [
          { text: '安装方法', link: langName + '/tutorial/install' },
          { text: '对象的开关控制', link: langName + '/tutorial/toggle' },
          { text: '服装切换', link: langName + '/tutorial/costume' },
          { text: '角色亮度调整', link: langName + '/tutorial/lightchanger' },
          { text: '角色体型调整', link: langName + '/tutorial/morph' },
          { text: '菜单整理', link: langName + '/tutorial/menu' }
        ]
      },
      {
        text: '文档',
        link: langName + '/docs/',
        collapsed: false,
        items: [
          {
            text: '组件列表', 
            link: langName + '/docs/components',
            items: [
              { text: '<img class="emoji" draggable="false" src="/lilycalInventory/images/LI_Script_AutoDresser.png"> LI AutoDresser', link: langName + '/docs/components/autodresser' },
              { text: '<img class="emoji" draggable="false" src="/lilycalInventory/images/LI_Script_AutoDresserSettings.png"> LI AutoDresserSettings', link: langName + '/docs/components/autodressersettings' },
              { text: '<img class="emoji" draggable="false" src="/lilycalInventory/images/LI_Script_AutoFixMeshSettings.png"> LI AutoFixMeshSettings', link: langName + '/docs/components/autofixmeshsettings' },
              { text: '<img class="emoji" draggable="false" src="/lilycalInventory/images/LI_Script_Comment.png"> LI Comment', link: langName + '/docs/components/comment' },
              { text: '<img class="emoji" draggable="false" src="/lilycalInventory/images/LI_Script_CostumeChanger.png"> LI CostumeChanger', link: langName + '/docs/components/costumechanger' },
              { text: '<img class="emoji" draggable="false" src="/lilycalInventory/images/LI_Script_ItemToggler.png"> LI ItemToggler', link: langName + '/docs/components/itemtoggler' },
              { text: '<img class="emoji" draggable="false" src="/lilycalInventory/images/LI_Script_Material.png"> LI MaterialModifier', link: langName + '/docs/components/materialmodifier' },
              { text: '<img class="emoji" draggable="false" src="/lilycalInventory/images/LI_Script_MaterialOptimizer.png"> LI MaterialOptimizer', link: langName + '/docs/components/materialoptimizer' },
              { text: '<img class="emoji" draggable="false" src="/lilycalInventory/images/LI_Script_MenuFolder.png"> LI MenuFolder', link: langName + '/docs/components/menufolder' },
              { text: '<img class="emoji" draggable="false" src="/lilycalInventory/images/LI_Script_Preset.png"> LI Preset', link: langName + '/docs/components/preset' },
              { text: '<img class="emoji" draggable="false" src="/lilycalInventory/images/LI_Script_Prop.png"> LI Prop', link: langName + '/docs/components/prop' },
              { text: '<img class="emoji" draggable="false" src="/lilycalInventory/images/LI_Script_SmoothChanger.png"> LI SmoothChanger', link: langName + '/docs/components/smoothchanger' }
            ]
          },
          { text: '使用 Direct Blend Tree 进行优化', link: langName + '/docs/directblendtree' },
          { text: '与 NDMF 和其他工具的兼容性', link: langName + '/docs/compatibility' },
          { text: '附带预制体说明', link: langName + '/docs/prefabs' }
        ]
      },
      {
        text: 'API',
        link: langName + '/api/',
        collapsed: false,
        items: [
          { text: 'API', link: langName + '/api/' }
        ]
      }
    ],

    search: {
      provider: 'local',
      options: {
        locales: {
          zh: {
            translations: {
              button: {
                buttonText: '搜索',
                buttonAriaLabel: '搜索'
              },
              modal: {
                noResultsText: '没有找到相关结果',
                resetButtonTitle: '清除搜索条件',
                footer: {
                  selectText: '选择',
                  navigateText: '切换'
                }
              }
            }
          }
        }
      }
    },

    editLink: {
      pattern: 'https://github.com/lilxyzw/lilycalInventory-docs/edit/main/docs/:path',
      text: '在 GitHub 上编辑此页'
    },

    footer: {
      message: '基于 MIT 许可发布',
      copyright: 'Copyright © 2024 lilxyzw'
    },

    docFooter: {
      prev: '上一页',
      next: '下一页'
    },

    outline: {
      label: '页面导航'
    },

    lastUpdated: {
      text: '最后更新于',
      formatOptions: {
        dateStyle: 'full',
        timeStyle: 'medium'
      }
    },

    langMenuLabel: '切换语言',
    returnToTopLabel: '返回顶部',
    sidebarMenuLabel: '菜单',
    darkModeSwitchLabel: '主题',
    lightModeSwitchTitle: '切换到浅色模式',
    darkModeSwitchTitle: '切换到深色模式'
  }
}) 